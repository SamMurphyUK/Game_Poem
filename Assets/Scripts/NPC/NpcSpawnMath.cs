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
}
