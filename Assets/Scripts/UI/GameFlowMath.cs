// Thresholds for the mid-game overlay copy. Times are seconds of play.
public static class GameFlowMath
{
    public const float LoveCountSeconds = 240f;
    public const float EscapePromptSeconds = 270f;
    public const float FadeSeconds = 1.5f;
    public const float SlowingIntervalSeconds = 120f;
    public const float SlowingHoldSeconds = 3f;
    public const float SlowingTailDelaySeconds = 1.1f;

    public const string LoveCountLine = "Roughly 100 triangles have found love. Are you struggling?";
    public const string EscapePromptLine = "Press Escape to end your suffering.";
    public const string SlowingLine = ">1,000 Triangles spawning. Computer....Slowing.";
    public const string SlowingLead = ">1,000 Triangles spawning.";
    public const string SlowingTail = "Computer....Slowing.";
    public const string StartMenuScene = "StartMenu";
    public const string PlayScene = "make no mistakes";

    public static bool ShouldShowLoveCount(float elapsedSeconds)
    {
        return elapsedSeconds >= LoveCountSeconds;
    }

    public static bool ShouldShowEscapePrompt(float elapsedSeconds)
    {
        return elapsedSeconds >= EscapePromptSeconds;
    }

    public static bool CanEndRun(float elapsedSeconds)
    {
        return elapsedSeconds >= EscapePromptSeconds;
    }

    public static bool ShouldTriggerSlowing(float elapsedSeconds, float nextAt)
    {
        return elapsedSeconds >= nextAt;
    }

    public static float FollowingSlowingAt(float currentNextAt, float intervalSeconds)
    {
        if (intervalSeconds <= 0f)
        {
            return currentNextAt;
        }

        return currentNextAt + intervalSeconds;
    }

    public static string SlowingTextAt(float holdElapsed)
    {
        if (holdElapsed < SlowingTailDelaySeconds)
        {
            return SlowingLead;
        }

        return SlowingLead + "\n\n" + SlowingTail;
    }

    public static bool IsWinTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return false;
        }

        return typeName.EndsWith("Win");
    }

    public static bool IsPlayerWin(bool hostIsPlayer, bool otherIsPlayer, bool resultIsWin)
    {
        return resultIsWin && (hostIsPlayer || otherIsPlayer);
    }

    public static float FadeAlpha(float fadeElapsed, float fadeSeconds)
    {
        if (fadeSeconds <= 0f)
        {
            return 1f;
        }

        if (fadeElapsed <= 0f)
        {
            return 0f;
        }

        if (fadeElapsed >= fadeSeconds)
        {
            return 1f;
        }

        return fadeElapsed / fadeSeconds;
    }
}
