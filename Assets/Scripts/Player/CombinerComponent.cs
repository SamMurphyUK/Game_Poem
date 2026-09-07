using UnityEngine;
using System.Collections;

public class CombinerComponent : MonoBehaviour
{
    [SerializeField] public CombinationRuleSO currentType;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float stoppingDistance = 0.1f;

    private Rigidbody2D rb;
    private bool isMovingToCombine = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            Debug.Log("Collided with: " + other.name);
            var playerCombiner = other.GetComponent<CombinerComponent>();

            if (playerCombiner != null && playerCombiner.currentType.GetMatchingType() == currentType && !isMovingToCombine)
            {
                StartCoroutine(CombineAnchorPoints(other.gameObject));
            }
        }
    }

    private IEnumerator CombineAnchorPoints(GameObject other)
    {
        isMovingToCombine = true;
        other.GetComponent<CombinerComponent>().isMovingToCombine = true;

        Transform thisAnchor = transform.Find("AnchorPoint");
        Transform otherAnchor = other.transform.Find("AnchorPoint");

        if (thisAnchor == null || otherAnchor == null)
        {
            Debug.LogWarning("Anchorpoint not found on one or both objects");
            isMovingToCombine = false;
            other.GetComponent<CombinerComponent>().isMovingToCombine = false;
            yield break;
        }

        // Move and rotate both objects towards each other until anchorpoints meet
        while (Vector3.Distance(thisAnchor.position, otherAnchor.position) > stoppingDistance)
        {
            // Movement: move each object towards the other's anchorpoint
            Vector3 thisToOtherDirection = (otherAnchor.position - thisAnchor.position).normalized;
            transform.position += thisToOtherDirection * movementSpeed * Time.deltaTime;

            Vector3 otherToThisDirection = (thisAnchor.position - otherAnchor.position).normalized;
            other.transform.position += otherToThisDirection * movementSpeed * Time.deltaTime;

            // Rotation: rotate each object to face towards the other's anchorpoint
            RotateTowardsDirection(transform, thisToOtherDirection);
            RotateTowardsDirection(other.transform, otherToThisDirection);

            yield return null;
        }

        // Get the result before combining
        CombinationRuleSO result = currentType.GetResult();

        if (result != null)
        {
            // Combine both objects into the result
            Combine(result);
            other.GetComponent<CombinerComponent>().Combine(result);
        }
        else
        {
            Debug.LogError("Combination result is null for: " + currentType.name);
        }

        isMovingToCombine = false;
        other.GetComponent<CombinerComponent>().isMovingToCombine = false;
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
}
