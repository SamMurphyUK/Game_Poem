using UnityEngine;
using System.Collections;

public enum CombinationStage
{
    Stage1,
    Stage2,
    Stage3
}

public class CombinerComponent : MonoBehaviour
{
    [SerializeField] public CombinationRuleSO currentType;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float stoppingDistance = 0.1f;
    [SerializeField] private float timeBeforeDetatch = 3f;

    private Rigidbody2D rb;
    private PlayerController controller;
    private bool isMovingToCombine = false;
    private bool isAttached = false;
    private CombinerComponent partner;
    private AudioSource audioSource;

    private RigidbodyType2D bodyTypeBeforeAttach;
    private bool controllerEnabledBeforeAttach;

    private CombinationStage currentStage = CombinationStage.Stage1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
    }

    public bool IsMovingToCombine()
    {
        return isMovingToCombine;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("NPC"))
        {
            return;
        }

        Debug.Log("Collided with: " + other.name);

        CombinerComponent otherCombiner = other.GetComponent<CombinerComponent>();

        if (!CanCombineWith(otherCombiner))
        {
            return;
        }

        // Both colliders of a pair get this callback, and more than one partner can
        // overlap us in the same physics step, so the roles are picked from the
        // objects themselves rather than from whichever callback happened to run first.
        CombinerComponent host = ChooseHost(this, otherCombiner);
        CombinerComponent attachment = host == this ? otherCombiner : this;

        host.StartCombine(attachment);
    }

    private bool CanCombineWith(CombinerComponent otherCombiner)
    {
        if (otherCombiner == null || otherCombiner == this)
        {
            return false;
        }

        if (IsBusy() || otherCombiner.IsBusy())
        {
            return false;
        }

        if (currentType == null || otherCombiner.currentType == null)
        {
            return false;
        }

        return otherCombiner.currentType.GetMatchingType() == currentType
            || currentType.GetMatchingType() == otherCombiner.currentType;
    }

    private bool IsBusy()
    {
        return isMovingToCombine || isAttached || partner != null;
    }

    // Whoever keeps driving its own movement has to stay the parent: a driven object
    // that gets parented to a partner loses control of its transform for good.
    private static CombinerComponent ChooseHost(CombinerComponent a, CombinerComponent b)
    {
        bool aIsDriven = a.controller != null;
        bool bIsDriven = b.controller != null;

        if (aIsDriven != bIsDriven)
        {
            return aIsDriven ? a : b;
        }

        return a.GetEntityId() <= b.GetEntityId() ? a : b;
    }

    private void StartCombine(CombinerComponent attachment)
    {
        // Claim both sides before the first yield so another overlapping partner
        // cannot start a second combine against either of us.
        ClaimPair(attachment);
        StartCoroutine(CombineAnchorPoints(attachment));
    }

    private IEnumerator CombineAnchorPoints(CombinerComponent attachment)
    {
        audioSource?.Play();

        Transform thisAnchor = transform.Find("AnchorPoint");
        Transform otherAnchor = attachment.transform.Find("AnchorPoint");

        if (thisAnchor == null || otherAnchor == null)
        {
            Debug.LogWarning("Anchorpoint not found on one or both objects");
            ReleasePair(attachment);
            yield break;
        }

        // Move and rotate both objects towards each other until anchorpoints meet
        while (Vector3.Distance(thisAnchor.position, otherAnchor.position) > stoppingDistance)
        {
            // Movement: move each object towards the other's anchorpoint
            Vector3 thisToOtherDirection = (otherAnchor.position - thisAnchor.position).normalized;
            transform.position += thisToOtherDirection * movementSpeed * Time.deltaTime;

            Vector3 otherToThisDirection = (thisAnchor.position - otherAnchor.position).normalized;
            attachment.transform.position += otherToThisDirection * movementSpeed * Time.deltaTime;

            // Rotation: rotate each object to face towards the other's anchorpoint
            RotateTowardsDirection(transform, thisToOtherDirection);
            RotateTowardsDirection(attachment.transform, otherToThisDirection);

            yield return null;
        }

        // Get the result before combining
        CombinationRuleSO result = currentType.GetResult();

        if (result == null)
        {
            result = attachment.currentType.GetResult();
        }

        if (result == null)
        {
            Debug.LogError("Combination result is null for: " + currentType.name);
            ReleasePair(attachment);
            yield break;
        }

        // Combine both objects into the result
        Combine(result);
        attachment.Combine(result);
        attachment.Attach(transform);

        isMovingToCombine = false;
        attachment.isMovingToCombine = false;

        yield return new WaitForSeconds(timeBeforeDetatch);

        if (attachment != null)
        {
            attachment.Detach();
        }

        AdvanceStage();
        ReleasePair(attachment);
    }

    private void RotateTowardsDirection(Transform target, Vector3 direction)
    {
        if (direction.magnitude < 0.01f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        target.rotation = Quaternion.RotateTowards(target.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void Combine(CombinationRuleSO outResult)
    {
        if (outResult == null)
        {
            Debug.LogError("Cannot combine with null result");
            return;
        }

        currentType = outResult;
        Debug.Log("Combined into: " + outResult.name);
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && outResult.sprite != null)
        {
            spriteRenderer.sprite = outResult.sprite;
        }
    }

    private void Attach(Transform host)
    {
        isAttached = true;

        if (controller != null)
        {
            controllerEnabledBeforeAttach = controller.enabled;
            controller.enabled = false;
        }

        if (rb != null)
        {
            bodyTypeBeforeAttach = rb.bodyType;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        transform.SetParent(host, worldPositionStays: true);
    }

    private void Detach()
    {
        transform.SetParent(null, worldPositionStays: true);

        // Everything Attach turned off has to come back on here, otherwise the
        // detached object stays frozen for the rest of the level.
        if (rb != null)
        {
            rb.bodyType = bodyTypeBeforeAttach;
            rb.linearVelocity = Vector2.zero;
        }

        if (controller != null)
        {
            controller.enabled = controllerEnabledBeforeAttach;
        }

        isAttached = false;
    }

    private void ClaimPair(CombinerComponent attachment)
    {
        partner = attachment;
        isMovingToCombine = true;

        attachment.partner = this;
        attachment.isMovingToCombine = true;
    }

    private void ReleasePair(CombinerComponent attachment)
    {
        partner = null;
        isMovingToCombine = false;

        if (attachment != null)
        {
            attachment.partner = null;
            attachment.isMovingToCombine = false;
        }
    }

    private void AdvanceStage()
    {
        if (currentStage < CombinationStage.Stage3)
        {
            currentStage++;
        }
    }

    public CombinationStage GetCurrentStage()
    {
        return currentStage;
    }
}
