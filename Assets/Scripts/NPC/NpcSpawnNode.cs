using System.Collections.Generic;
using UnityEngine;

// Drop this in a scene (or the NPC Spawn Node prefab) to keep a pool of
// wandering NPCs around the marker. They spawn at random offsets and walk
// until they bounce off the level.
public class NpcSpawnNode : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Vector3 spawnScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private float spawnZ = 1f;

    [Header("Population")]
    [SerializeField] private int minAlive = 2;
    [SerializeField] private int maxAlive = 6;
    [SerializeField] private int spawnOnStart = 2;
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private float maxSpawnInterval = 4f;

    [Header("Placement")]
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnClearance = 6f;
    [SerializeField] private LayerMask blockedLayers = ~0;
    [SerializeField] private int placementTries = 8;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 0f;

    [Header("Emotions")]
    [SerializeField] private List<NpcSpawnEntry> spawnTable = new List<NpcSpawnEntry>();

    private readonly List<GameObject> live = new List<GameObject>();
    private float spawnTimer;

    private void Start()
    {
        int initial = NpcSpawnMath.ClampInitialSpawn(spawnOnStart, maxAlive);
        for (int i = 0; i < initial; i++)
        {
            TrySpawn();
        }

        spawnTimer = NpcSpawnMath.NextInterval(minSpawnInterval, maxSpawnInterval, Random.value);
    }

    private void Update()
    {
        ForgetMissing();

        spawnTimer -= Time.deltaTime;
        bool due = spawnTimer <= 0f;
        bool refill = NpcSpawnMath.NeedsRefill(live.Count, minAlive);
        if ((due || refill) && NpcSpawnMath.CanKeepSpawning(live.Count, maxAlive))
        {
            TrySpawn();
            spawnTimer = NpcSpawnMath.NextInterval(minSpawnInterval, maxSpawnInterval, Random.value);
        }
        else if (due)
        {
            spawnTimer = NpcSpawnMath.NextInterval(minSpawnInterval, maxSpawnInterval, Random.value);
        }
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        for (int i = 0; i < live.Count; i++)
        {
            if (live[i] != null)
            {
                Destroy(live[i]);
            }
        }

        live.Clear();
    }

    private CombinationRuleSO PickSpawnType()
    {
        if (spawnTable == null || spawnTable.Count == 0)
        {
            return null;
        }

        float[] weights = new float[spawnTable.Count];
        for (int i = 0; i < spawnTable.Count; i++)
        {
            NpcSpawnEntry entry = spawnTable[i];
            if (entry == null || entry.type == null)
            {
                weights[i] = 0f;
            }
            else
            {
                weights[i] = entry.weight;
            }
        }

        int index = NpcSpawnMath.PickWeightedIndex(weights, Random.value);
        if (index < 0)
        {
            return null;
        }

        return spawnTable[index].type;
    }

    public bool TrySpawn()
    {
        if (npcPrefab == null || !NpcSpawnMath.CanKeepSpawning(live.Count, maxAlive))
        {
            return false;
        }

        if (!TryFindClearPoint(out Vector3 point))
        {
            return false;
        }

        GameObject npc = Instantiate(npcPrefab, point, Quaternion.identity);
        npc.transform.localScale = spawnScale;

        CombinationRuleSO type = PickSpawnType();
        CombinerComponent combiner = npc.GetComponent<CombinerComponent>();
        if (type != null && combiner != null)
        {
            combiner.ApplyType(type);
            npc.name = type.name;
            if (type.name != null && type.name.StartsWith("Light"))
            {
                WhiteTriangleBacking.Ensure(npc);
            }
        }
        else
        {
            npc.name = npcPrefab.name;
        }

        NpcWander wander = npc.GetComponent<NpcWander>();
        if (wander == null)
        {
            wander = npc.AddComponent<NpcWander>();
        }

        wander.ConfigureHome(transform, wanderRadius);
        live.Add(npc);
        return true;
    }

    private bool TryFindClearPoint(out Vector3 point)
    {
        int tries = Mathf.Max(1, placementTries);
        for (int i = 0; i < tries; i++)
        {
            Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, spawnRadius);
            point = NpcSpawnMath.SpawnPoint(transform.position, offset, spawnZ);
            if (IsClear(point))
            {
                return true;
            }
        }

        point = NpcSpawnMath.SpawnPoint(transform.position, Vector2.zero, spawnZ);
        return IsClear(point);
    }

    private bool IsClear(Vector3 point)
    {
        if (spawnClearance <= 0f)
        {
            return true;
        }

        Collider2D hit = Physics2D.OverlapCircle(point, spawnClearance, blockedLayers);
        return hit == null;
    }

    private void ForgetMissing()
    {
        for (int i = live.Count - 1; i >= 0; i--)
        {
            if (live[i] == null)
            {
                live.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.85f, 0.75f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.4f, spawnRadius));
        Gizmos.DrawSphere(transform.position, 0.45f);

        if (wanderRadius > 0f)
        {
            Gizmos.color = new Color(0.95f, 0.75f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
        }
    }
}

[System.Serializable]
public class NpcSpawnEntry
{
    public CombinationRuleSO type;
    public float weight = 1f;
}
