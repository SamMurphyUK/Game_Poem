using UnityEngine;

// Spawn-count and placement helpers for NpcSpawnNode.
public static class NpcSpawnMath
{
    public static bool CanKeepSpawning(int alive, int maxAlive)
    {
        return maxAlive > 0 && alive < maxAlive;
    }

    public static bool NeedsRefill(int alive, int minAlive)
    {
        return alive < Mathf.Max(0, minAlive);
    }

    public static int ClampInitialSpawn(int spawnOnStart, int maxAlive)
    {
        if (maxAlive <= 0)
        {
            return 0;
        }

        return Mathf.Clamp(spawnOnStart, 0, maxAlive);
    }

    public static Vector3 SpawnPoint(Vector3 origin, Vector2 offset, float z)
    {
        return new Vector3(origin.x + offset.x, origin.y + offset.y, z);
    }

    public static float NextInterval(float minSeconds, float maxSeconds, float roll01)
    {
        float min = Mathf.Max(0.05f, minSeconds);
        float max = Mathf.Max(min, maxSeconds);
        return Mathf.Lerp(min, max, Mathf.Clamp01(roll01));
    }

    // roll01 is 0..1. Zero and negative weights are skipped. A roll of 1 lands
    // on the last positive entry.
    public static int PickWeightedIndex(float[] weights, float roll01)
    {
        if (weights == null || weights.Length == 0)
        {
            return -1;
        }

        float total = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i] > 0f)
            {
                total += weights[i];
            }
        }

        if (total <= 0f)
        {
            return -1;
        }

        float roll = Mathf.Clamp01(roll01) * total;
        float acc = 0f;
        int last = -1;
        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i] <= 0f)
            {
                continue;
            }

            last = i;
            acc += weights[i];
            if (roll < acc)
            {
                return i;
            }
        }

        return last;
    }
}
