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

    private RigidbodyType2D bodyTypeBeforeCombine;
    private bool hasStoredBodyType;
    private bool simulatedBeforeAttach = true;
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

        // Rotate first, then close. Doing both at once with the anchors sitting a
        // unit off-centre makes the gap swing as fast as it shrinks, which is the
        // visual jitter even after the bodies stop fighting the solver.
        float approachTimeout = 3f;
        float elapsed = 0f;

        while (elapsed < approachTimeout)
        {
            Vector3 between = Flatten(attachment.transform.position - transform.position);
            if (between.sqrMagnitude < 0.0001f)
            {
                break;
            }

            RotateTowardsDirection(transform, between);
            RotateTowardsDirection(attachment.transform, -between);

            if (Faces(transform, between) && Faces(attachment.transform, -between))
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        float gap = AnchorGap(thisAnchor, otherAnchor);
        while (gap > stoppingDistance && elapsed < approachTimeout)
        {
            float step = Mathf.Min(movementSpeed * Time.deltaTime, gap * 0.5f);
            Vector3 close = Flatten(otherAnchor.position - thisAnchor.position).normalized;
            transform.position += close * step;
            attachment.transform.position -= close * step;

            elapsed += Time.deltaTime;
            yield return null;
            gap = AnchorGap(thisAnchor, otherAnchor);
        }

        // Seat whatever the approach could not, so the pair always ends up together
        // rather than a fraction of a unit apart.
        attachment.transform.position += Flatten(thisAnchor.position - otherAnchor.position);

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

        // The host drives itself again from here, so it needs its own body back.
        // The attachment stays kinematic until it detaches.
        RestoreBody();

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

    private static Vector3 Flatten(Vector3 value)
    {
        value.z = 0f;
        return value;
    }

    private static float AnchorGap(Transform a, Transform b)
    {
        return Flatten(a.position - b.position).magnitude;
    }

    private bool Faces(Transform target, Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Mathf.Abs(Mathf.DeltaAngle(target.eulerAngles.z, angle)) < 1f;
    }

    private void RotateTowardsDirection(Transform target, Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

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

        BeginScriptedMove();

        // Unity 2D will not simulate a Rigidbody2D that is parented to another
        // Rigidbody2D. Turning simulation off here folds this collider into the
        // host so the pair moves as one body instead of two fighting solvers.
        if (rb != null)
        {
            simulatedBeforeAttach = rb.simulated;
            rb.simulated = false;
        }

        transform.SetParent(host, worldPositionStays: true);
    }

    private void Detach()
    {
        transform.SetParent(null, worldPositionStays: true);

        if (rb != null)
        {
            rb.simulated = simulatedBeforeAttach;
        }

        // Everything Attach turned off has to come back on here, otherwise the
        // detached object stays frozen for the rest of the level.
        RestoreBody();

        if (controller != null)
        {
            controller.enabled = controllerEnabledBeforeAttach;
        }

        isAttached = false;
    }

    // A scripted move writes transform.position outright, which a dynamic body
    // treats as a teleport and the solver answers by shoving everything back out
    // of the overlap it just created. Handing the body over as kinematic for the
    // duration is what makes the approach read as one smooth movement.
    private void BeginScriptedMove()
    {
        if (rb == null)
        {
            return;
        }

        if (!hasStoredBodyType)
        {
            bodyTypeBeforeCombine = rb.bodyType;
            hasStoredBodyType = true;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void RestoreBody()
    {
        if (rb == null)
        {
            return;
        }

        if (hasStoredBodyType)
        {
            rb.bodyType = bodyTypeBeforeCombine;
            hasStoredBodyType = false;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    // Combining seats two solid colliders inside one another on purpose, so for as
    // long as the pair is joined they must not try to push each other apart.
    private void SetPairCollisions(CombinerComponent other, bool ignore)
    {
        if (other == null)
        {
            return;
        }

        Collider2D[] mine = GetComponentsInChildren<Collider2D>();
        Collider2D[] theirs = other.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D a in mine)
        {
            if (a == null || a.isTrigger)
            {
                continue;
            }

            foreach (Collider2D b in theirs)
            {
                if (b == null || b.isTrigger)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(a, b, ignore);
            }
        }
    }

    private void ClaimPair(CombinerComponent attachment)
    {
        partner = attachment;
        isMovingToCombine = true;

        attachment.partner = this;
        attachment.isMovingToCombine = true;

        BeginScriptedMove();
        attachment.BeginScriptedMove();
        SetPairCollisions(attachment, ignore: true);
    }

    private void ReleasePair(CombinerComponent attachment)
    {
        partner = null;
        isMovingToCombine = false;
        RestoreBody();

        if (attachment != null)
        {
            SetPairCollisions(attachment, ignore: false);
            attachment.partner = null;
            attachment.isMovingToCombine = false;
            attachment.RestoreBody();
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
