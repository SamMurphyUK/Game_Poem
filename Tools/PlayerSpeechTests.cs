using System;

public static class PlayerSpeechTests
{
    static int failures;

    public static int Main()
    {
        ExpectEqual(40f, PlayerSpeechMath.NextWaitSeconds(0f), "wait at roll 0");
        ExpectEqual(60f, PlayerSpeechMath.NextWaitSeconds(0.5f), "wait at roll 0.5");
        ExpectEqual(80f, PlayerSpeechMath.NextWaitSeconds(1f), "wait at roll 1");
        ExpectEqual(40f, PlayerSpeechMath.NextWaitSeconds(-2f), "wait clamps low");
        ExpectEqual(80f, PlayerSpeechMath.NextWaitSeconds(3f), "wait clamps high");

        Expect(PlayerSpeechMath.Stage1Lines.Length == 2, "stage 1 count");
        Expect(PlayerSpeechMath.Stage2Lines.Length == 3, "stage 2 count");
        Expect(PlayerSpeechMath.Stage3Lines.Length == 4, "stage 3 count");
        Expect(Has(PlayerSpeechMath.LinesFor(0), "Circles have all the fun."), "stage 1 circles");
        Expect(Has(PlayerSpeechMath.LinesFor(1), "I deserve more."), "stage 2 deserve");
        Expect(Has(PlayerSpeechMath.LinesFor(2), "Don't give up now."), "stage 3 don't give up");
        Expect(Has(PlayerSpeechMath.LinesFor(1), "Did that triangle pinch my bum?"), "stage 2 pinch");
        Expect(Has(PlayerSpeechMath.LinesFor(2), "Did that triangle pinch my bum?"), "stage 3 pinch");
        Expect(!Has(PlayerSpeechMath.LinesFor(0), "Did that triangle pinch my bum?"), "stage 1 has no pinch");

        ExpectEqual(-1f, PlayerSpeechMath.PickLineIndex(0, 0.2f, -1), "empty list");
        ExpectEqual(0f, PlayerSpeechMath.PickLineIndex(1, 0.9f, 0), "single line");
        ExpectEqual(0f, PlayerSpeechMath.PickLineIndex(3, 0f, -1), "first bucket");
        ExpectEqual(2f, PlayerSpeechMath.PickLineIndex(3, 0.99f, -1), "last bucket");
        ExpectEqual(1f, PlayerSpeechMath.PickLineIndex(3, 0.1f, 0), "skips last");

        if (failures > 0)
        {
            Console.Error.WriteLine("FAILED " + failures);
            return 1;
        }

        Console.WriteLine("PlayerSpeechTests passed");
        return 0;
    }

    static bool Has(string[] lines, string line)
    {
        if (lines == null)
        {
            return false;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == line)
            {
                return true;
            }
        }

        return false;
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
