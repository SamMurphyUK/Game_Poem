using UnityEngine;

// Shared NPC and player bark timing, plus the stage-3 wall openings. NPC
// cooldown is global so one triangle talking blocks every other NPC. Player
// idle lines share a second clock so either ship speaks about once a minute.
public static class DialogueClock
{
    private static float nextNpcSpeakTime;
    private static float nextPlayerSpeakTime;
    private static bool openedStage3Walls;
    private static bool scheduledFirstPlayerWait;

    private static PlayerController[] cachedPlayers;
    private static float nextPlayerCacheTime;
    private static CombinationStage cachedHighestStage;
    private static int cachedHighestFrame = -1;
    private const float PlayerCacheSeconds = 0.5f;

    public static float NpcCooldown(CombinationStage stage)
    {
        switch (stage)
        {
            case CombinationStage.Stage2:
                return 4f;
            case CombinationStage.Stage3:
                return 20f;
            default:
                return 10f;
        }
    }

    public static bool CanNpcSpeak(float now)
    {
        return now >= nextNpcSpeakTime;
    }

    public static void MarkNpcSpoke(float now, CombinationStage stage)
    {
        nextNpcSpeakTime = now + NpcCooldown(stage);
    }

    // Combining just finished: NPCs may speak again instead of waiting out a
    // cooldown that started before the join.
    public static void AllowNpcSpeakAfterCombine(float now)
    {
        nextNpcSpeakTime = now;
    }

    public static bool CanPlayerSpeak(float now)
    {
        return now >= nextPlayerSpeakTime;
    }

    public static void EnsureFirstPlayerWait(float now, float waitSeconds)
    {
        if (scheduledFirstPlayerWait)
        {
            return;
        }

        scheduledFirstPlayerWait = true;
        if (waitSeconds < 0f)
        {
            waitSeconds = 0f;
        }

        nextPlayerSpeakTime = now + waitSeconds;
    }

    public static bool TryClaimPlayerSpeak(float now, float waitSeconds)
    {
        if (now < nextPlayerSpeakTime)
        {
            return false;
        }

        MarkPlayerSpoke(now, waitSeconds);
        return true;
    }

    public static void MarkPlayerSpoke(float now, float waitSeconds)
    {
        if (waitSeconds < 0f)
        {
            waitSeconds = 0f;
        }

        nextPlayerSpeakTime = now + waitSeconds;
        scheduledFirstPlayerWait = true;
    }

    public static PlayerController[] Players(float now)
    {
        if (cachedPlayers == null || now >= nextPlayerCacheTime)
        {
            cachedPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            nextPlayerCacheTime = now + PlayerCacheSeconds;
        }

        return cachedPlayers;
    }

    public static CombinationStage HighestPlayerStage()
    {
        int frame = Time.frameCount;
        if (cachedHighestFrame == frame)
        {
            return cachedHighestStage;
        }

        CombinationStage highest = CombinationStage.Stage1;
        PlayerController[] players = Players(Time.time);
        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];
            if (player == null)
            {
                continue;
            }

            CombinerComponent combiner = player.GetComponent<CombinerComponent>();
            if (combiner == null)
            {
                continue;
            }

            CombinationStage stage = combiner.GetCurrentStage();
            if (stage > highest)
            {
                highest = stage;
            }
        }

        cachedHighestStage = highest;
        cachedHighestFrame = frame;
        return highest;
    }

    public static bool IsStage3Wall(string name)
    {
        return name == "Wall (16)"
            || name == "Wall (17)"
            || name == "Wall (142)"
            || name == "Wall (143)";
    }

    public static void NotifyStage(CombinationStage stage)
    {
        if (stage < CombinationStage.Stage3 || openedStage3Walls)
        {
            return;
        }

        openedStage3Walls = true;
        OpenNamedWall("Wall (16)");
        OpenNamedWall("Wall (17)");
        OpenNamedWall("Wall (142)");
        OpenNamedWall("Wall (143)");
    }

    private static void OpenNamedWall(string name)
    {
        GameObject wall = GameObject.Find(name);
        if (wall == null)
        {
            return;
        }

        Collider2D[] colliders = wall.GetComponents<Collider2D>();
        for (int c = 0; c < colliders.Length; c++)
        {
            if (colliders[c] != null)
            {
                colliders[c].enabled = false;
            }
        }
    }

    public static void ResetForTests()
    {
        nextNpcSpeakTime = 0f;
        nextPlayerSpeakTime = 0f;
        scheduledFirstPlayerWait = false;
        openedStage3Walls = false;
        cachedPlayers = null;
        nextPlayerCacheTime = 0f;
        cachedHighestFrame = -1;
    }
}
