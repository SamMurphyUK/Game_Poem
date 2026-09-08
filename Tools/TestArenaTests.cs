using System;

public static class TestArenaTests
{
    static int failures;

    public static int Main()
    {
        ExpectEqual(0f, TestCombinationMath.SlotX(0, 1, 3.2f), "single slot");
        ExpectEqual(-3.2f, TestCombinationMath.SlotX(0, 3, 3.2f), "first of three");
        ExpectEqual(0f, TestCombinationMath.SlotX(1, 3, 3.2f), "middle of three");
        ExpectEqual(3.2f, TestCombinationMath.SlotX(2, 3, 3.2f), "last of three");

        if (failures > 0)
        {
            Console.Error.WriteLine("FAILED " + failures);
            return 1;
        }

        Console.WriteLine("TestArenaTests passed");
        return 0;
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
