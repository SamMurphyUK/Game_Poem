using UnityEngine;

public class CombinerComponent : MonoBehaviour
{
    [SerializeField] public CombinationRuleSO currentType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("NPC"))
        {
            var playerCombiner = other.GetComponent<CombinerComponent>();

            if (playerCombiner != null && playerCombiner.currentType.GetMatchingType() == currentType)
            {
                Combine(currentType.GetResult());
            }
        }
    }

    private void Combine(CombinationRuleSO outResult)
    {
        currentType = outResult;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && outResult.sprite != null)
        {
            spriteRenderer.sprite = outResult.sprite;
        }
    }
}
