using System;

public static class GameFlowTests
{
    static int failures;

    public static int Main()
    {
        Expect(!GameFlowMath.ShouldShowLoveCount(239.9f), "before 4 minutes");
        Expect(GameFlowMath.ShouldShowLoveCount(240f), "at 4 minutes");
        Expect(GameFlowMath.ShouldShowLoveCount(300f), "after 4 minutes");
        Expect(!GameFlowMath.ShouldShowEscapePrompt(269.9f), "before 4.5 minutes");
        Expect(GameFlowMath.ShouldShowEscapePrompt(270f), "at 4.5 minutes");
        Expect(!GameFlowMath.CanEndRun(269.9f), "escape locked before prompt");
        Expect(GameFlowMath.CanEndRun(270f), "escape open at prompt");
        ExpectEqual(0f, GameFlowMath.FadeAlpha(0f, 1.5f), "fade start");
        ExpectEqual(0.5f, GameFlowMath.FadeAlpha(0.75f, 1.5f), "fade mid");
        ExpectEqual(1f, GameFlowMath.FadeAlpha(1.5f, 1.5f), "fade end");
        ExpectEqual(1f, GameFlowMath.FadeAlpha(2f, 1.5f), "fade past end");
        Expect(GameFlowMath.LoveCountLine.Contains("100 triangles"), "love copy");
        Expect(GameFlowMath.EscapePromptLine.Contains("Escape"), "escape copy");

        if (failures > 0)
        {
            Console.Error.WriteLine("FAILED " + failures);
            return 1;
        }

        Console.WriteLine("GameFlowTests passed");
        return 0;
    }

    static void Expect(bool condition, string label)
    {
        if (!condition)
        {
            failures++;
            Console.Error.WriteLine("FAIL: " + label);
        }
    }

    static void ExpectEqual(float expected, float actual, string label)
    {
        if (Math.Abs(expected - actual) > 0.0001f)
        {
            failures++;
            Console.Error.WriteLine("FAIL: " + label + " expected " + expected + " got " + actual);
        }
    }
}
