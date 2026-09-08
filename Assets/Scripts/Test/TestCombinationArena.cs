using UnityEngine;

// TEST scene helper: drops every emotion type in two rows next to the players
// so combinations can be checked without waiting on the spawn nodes.
public class TestCombinationArena : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private CombinationRuleSO[] types;
    [SerializeField] private Vector3 npcScale = new Vector3(4f, 4f, 4f);
    [SerializeField] private float rowSpacing = 3.2f;
    [SerializeField] private float darkRowY = 3.5f;
    [SerializeField] private float lightRowY = -3.5f;
    [SerializeField] private float spawnZ = 1f;

    private void Start()
    {
        if (npcPrefab == null || types == null)
        {
            return;
        }

        int darkIndex = 0;
        int lightIndex = 0;
        int darkCount = 0;
        int lightCount = 0;
        for (int i = 0; i < types.Length; i++)
        {
            if (IsLight(types[i]))
            {
                lightCount++;
            }
            else
            {
                darkCount++;
            }
        }

        for (int i = 0; i < types.Length; i++)
        {
            CombinationRuleSO type = types[i];
            if (type == null)
            {
                continue;
            }

            bool light = IsLight(type);
            int index = light ? lightIndex++ : darkIndex++;
            int count = light ? lightCount : darkCount;
            float y = light ? lightRowY : darkRowY;
            float x = TestCombinationMath.SlotX(index, count, rowSpacing);
            Spawn(type, new Vector3(x, y, spawnZ));
        }
    }

    private void Spawn(CombinationRuleSO type, Vector3 position)
    {
        GameObject npc = Instantiate(npcPrefab, position, Quaternion.identity);
        npc.transform.localScale = npcScale;
        npc.name = type.name;

        CombinerComponent combiner = npc.GetComponent<CombinerComponent>();
        if (combiner != null)
        {
            combiner.ApplyType(type);
        }

        NpcWander wander = npc.GetComponent<NpcWander>();
        if (wander != null)
        {
            wander.enabled = false;
        }

        Rigidbody2D body = npc.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    private static bool IsLight(CombinationRuleSO type)
    {
        return type != null && type.name != null && type.name.StartsWith("Light");
    }
}
