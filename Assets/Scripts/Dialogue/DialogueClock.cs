using UnityEngine;

// Shared NPC bark timing and the stage-3 wall openings. Cooldown is global so
// one triangle talking blocks every other NPC for either player.
public static class DialogueClock
{
    private static float nextNpcSpeakTime;
    private static float nextPlayerSpeakTime;
    private static bool openedStage3Walls;
    private static bool scheduledFirstPlayerWait;

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

    public static CombinationStage HighestPlayerStage()
    {
        CombinationStage highest = CombinationStage.Stage1;
        CombinerComponent[] combiners = Object.FindObjectsByType<CombinerComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < combiners.Length; i++)
        {
            CombinerComponent combiner = combiners[i];
            if (combiner == null || combiner.GetComponent<PlayerController>() == null)
            {
                continue;
            }

            CombinationStage stage = combiner.GetCurrentStage();
            if (stage > highest)
            {
                highest = stage;
            }
        }

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
        Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform transform = transforms[i];
            if (transform == null || !IsStage3Wall(transform.name))
            {
                continue;
            }

            Collider2D[] colliders = transform.GetComponents<Collider2D>();
            for (int c = 0; c < colliders.Length; c++)
            {
                if (colliders[c] != null)
                {
                    colliders[c].enabled = false;
                }
            }
        }
    }

    public static void ResetForTests()
    {
        nextNpcSpeakTime = 0f;
        nextPlayerSpeakTime = 0f;
        scheduledFirstPlayerWait = false;
        openedStage3Walls = false;
    }
}
