// Thresholds for the mid-game overlay copy. Times are seconds of play.
public static class GameFlowMath
{
    public const float LoveCountSeconds = 240f;
    public const float EscapePromptSeconds = 270f;
    public const float FadeSeconds = 1.5f;

    public const string LoveCountLine = "Roughly 100 triangles have found love. Are you struggling?";
    public const string EscapePromptLine = "Press Escape to end your suffering.";
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
