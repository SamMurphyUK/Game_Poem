// Idle player barks. One shared cooldown so either triangle speaks about once
// a minute, not both on top of each other.
public static class PlayerSpeechMath
{
    public const float MeanSeconds = 60f;
    public const float JitterSeconds = 20f;
    public const string SpawnLine = "How gross.";

    public static readonly string[] Stage1Lines =
    {
        "Surely there must be ...",
        "Circles have all the fun."
    };

    public static readonly string[] Stage2Lines =
    {
        "I deserve more.",
        "It's still too crowded.",
        "Did that triangle pinch my bum?"
    };

    public static readonly string[] Stage3Lines =
    {
        "I hope they're doing just fine.",
        "Almost there.",
        "Don't give up now.",
        "Did that triangle pinch my bum?"
    };

    public static float NextWaitSeconds(float roll01)
    {
        if (roll01 < 0f)
        {
            roll01 = 0f;
        }
        else if (roll01 > 1f)
        {
            roll01 = 1f;
        }

        return MeanSeconds + (roll01 * 2f - 1f) * JitterSeconds;
    }

    public static string[] LinesFor(int stage)
    {
        if (stage >= 2)
        {
            return Stage3Lines;
        }

        if (stage == 1)
        {
            return Stage2Lines;
        }

        return Stage1Lines;
    }

    public static int PickLineIndex(int count, float roll01, int lastIndex)
    {
        if (count <= 0)
        {
            return -1;
        }

        if (count == 1)
        {
            return 0;
        }

        if (roll01 < 0f)
        {
            roll01 = 0f;
        }
        else if (roll01 > 1f)
        {
            roll01 = 1f;
        }

        int index = (int)(roll01 * count);
        if (index >= count)
        {
            index = count - 1;
        }

        if (index == lastIndex)
        {
            index = (index + 1) % count;
        }

        return index;
    }
}
