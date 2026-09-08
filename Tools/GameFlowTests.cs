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
        Expect(!GameFlowMath.ShouldTriggerSlowing(119.9f, 120f), "slowing not yet");
        Expect(GameFlowMath.ShouldTriggerSlowing(120f, 120f), "slowing at 2 minutes");
        ExpectEqual(240f, GameFlowMath.FollowingSlowingAt(120f, 120f), "next slowing");
        Expect(GameFlowMath.SlowingLine.Contains("1,000 Triangles"), "slowing copy");
        Expect(GameFlowMath.SlowingTextAt(0f) == GameFlowMath.SlowingLead, "slowing lead first");
        Expect(GameFlowMath.SlowingTextAt(1.09f) == GameFlowMath.SlowingLead, "slowing still lead");
        Expect(GameFlowMath.SlowingTextAt(1.1f).Contains(GameFlowMath.SlowingTail), "slowing tail after beat");
        Expect(GameFlowMath.SlowingLead + " " + GameFlowMath.SlowingTail == GameFlowMath.SlowingLine, "slowing lines join");
        Expect(GameFlowMath.IsWinTypeName("DarkWin"), "dark win name");
        Expect(GameFlowMath.IsWinTypeName("LightWin"), "light win name");
        Expect(!GameFlowMath.IsWinTypeName("DarkDesire"), "desire is not win");
        Expect(!GameFlowMath.IsWinTypeName(""), "empty is not win");
        Expect(GameFlowMath.IsPlayerWin(true, false, true), "player plus npc win");
        Expect(GameFlowMath.IsPlayerWin(false, true, true), "npc plus player win");
        Expect(!GameFlowMath.IsPlayerWin(false, false, true), "npc plus npc is not a player win");
        Expect(!GameFlowMath.IsPlayerWin(true, true, false), "players without win type");

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
