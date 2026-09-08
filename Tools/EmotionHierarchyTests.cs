using System;
using UnityEngine;

public static class EmotionHierarchyTests
{
    static int failures;

    public static int Main()
    {
        TestWeightedPickBasics();
        TestEvolvedFormsAreRare();
        TestDiagramPairs();
        TestCrossFamilyAndWinDoNotCombine();
        TestCombineResultsFollowTheDiagram();

        if (failures > 0)
        {
            Console.Error.WriteLine("FAILED " + failures);
            return 1;
        }

        Console.WriteLine("EmotionHierarchyTests passed");
        return 0;
    }

    static void TestWeightedPickBasics()
    {
        ExpectEqual(-1, NpcSpawnMath.PickWeightedIndex(null, 0.5f), "null weights");
        ExpectEqual(-1, NpcSpawnMath.PickWeightedIndex(new float[0], 0.5f), "empty weights");
        ExpectEqual(-1, NpcSpawnMath.PickWeightedIndex(new float[] { 0f, -1f }, 0.2f), "no positive weights");
        ExpectEqual(0, NpcSpawnMath.PickWeightedIndex(new float[] { 1f }, 0f), "single at 0");
        ExpectEqual(0, NpcSpawnMath.PickWeightedIndex(new float[] { 1f }, 1f), "single at 1");
        ExpectEqual(0, NpcSpawnMath.PickWeightedIndex(new float[] { 1f, 1f }, 0f), "first bucket");
        ExpectEqual(1, NpcSpawnMath.PickWeightedIndex(new float[] { 1f, 1f }, 0.6f), "second bucket");
        ExpectEqual(2, NpcSpawnMath.PickWeightedIndex(new float[] { 0f, 1f, 1f }, 0.6f), "skips zero");
        ExpectEqual(1, NpcSpawnMath.PickWeightedIndex(new float[] { 1f, 1f }, 1f), "roll 1 is last");
    }

    static void TestEvolvedFormsAreRare()
    {
        // Same weights as the NPC Spawn Node prefab: 78% base, 20% evolved, 2% win.
        float[] weights =
        {
            39f, 39f, 39f, 39f, 39f, 39f, 39f, 39f,
            20f, 20f, 20f, 20f,
            4f, 4f
        };

        int n = 10000;
        int bases = 0;
        int evolved = 0;
        int wins = 0;
        for (int i = 0; i < n; i++)
        {
            float roll = i / (float)n;
            int index = NpcSpawnMath.PickWeightedIndex(weights, roll);
            if (index < 0)
            {
                Fail("weighted pick returned -1");
                return;
            }

            if (index < 8)
            {
                bases++;
            }
            else if (index < 12)
            {
                evolved++;
            }
            else
            {
                wins++;
            }
        }

        Expect(bases + evolved + wins == n, "all rolls classified");
        Expect(evolved >= n * 0.18f && evolved <= n * 0.22f, "evolved ~20% (got " + evolved + ")");
        Expect(wins >= n * 0.015f && wins <= n * 0.025f, "wins ~2% (got " + wins + ")");
        Expect(bases >= n * 0.75f && bases <= n * 0.81f, "bases ~78% (got " + bases + ")");
        Console.WriteLine("spawn mix over " + n + ": base=" + bases + " evolved=" + evolved + " win=" + wins);
    }

    static void TestDiagramPairs()
    {
        Graph g = Graph.Build();
        Expect(g.darkPassion.CanCombineWith(g.darkDesire), "dark passion+desire");
        Expect(g.darkDesire.CanCombineWith(g.darkPassion), "dark desire+passion");
        Expect(g.darkEnvy.CanCombineWith(g.darkJoy), "dark envy+joy");
        Expect(g.darkJoy.CanCombineWith(g.darkEnvy), "dark joy+envy");
        Expect(g.darkPassionateDesire.CanCombineWith(g.darkEnviousJoy), "dark evolved pair");
        Expect(g.lightPassion.CanCombineWith(g.lightDesire), "light passion+desire");
        Expect(g.lightEnvy.CanCombineWith(g.lightJoy), "light envy+joy");
        Expect(g.lightDesiredPassion.CanCombineWith(g.lightJoyousEnvy), "light evolved pair");
    }

    static void TestCrossFamilyAndWinDoNotCombine()
    {
        Graph g = Graph.Build();
        Expect(!g.darkPassion.CanCombineWith(g.darkEnvy), "passion does not match envy");
        Expect(!g.darkPassion.CanCombineWith(g.lightDesire), "dark does not match light");
        Expect(!g.darkPassion.CanCombineWith(g.darkPassion), "same type does not match");
        Expect(!g.darkWin.CanCombineWith(g.darkPassionateDesire), "win does not match evolved");
        Expect(!g.darkWin.CanCombineWith(g.lightWin), "wins do not match");
        Expect(!g.darkPassionateDesire.CanCombineWith(g.lightJoyousEnvy), "evolved stays in family");
        Expect(!g.darkPassion.CanCombineWith(null), "null does not match");
    }

    static void TestCombineResultsFollowTheDiagram()
    {
        Graph g = Graph.Build();
        ExpectSame(g.darkPassionateDesire, g.darkPassion.ResultWith(g.darkDesire), "dark passion+desire result");
        ExpectSame(g.darkPassionateDesire, g.darkDesire.ResultWith(g.darkPassion), "dark desire+passion result");
        ExpectSame(g.darkEnviousJoy, g.darkEnvy.ResultWith(g.darkJoy), "dark envy+joy result");
        ExpectSame(g.darkWin, g.darkPassionateDesire.ResultWith(g.darkEnviousJoy), "dark win result");
        ExpectSame(g.lightDesiredPassion, g.lightPassion.ResultWith(g.lightDesire), "light passion+desire result");
        ExpectSame(g.lightJoyousEnvy, g.lightJoy.ResultWith(g.lightEnvy), "light joy+envy result");
        ExpectSame(g.lightWin, g.lightDesiredPassion.ResultWith(g.lightJoyousEnvy), "light win result");
        Expect(g.darkPassion.ResultWith(g.darkJoy) == null, "illegal pair has no result");
        Expect(g.darkWin.ResultWith(g.darkEnviousJoy) == null, "win has no further result");
        Expect(g.darkWin.IsWinType(), "dark win flagged");
        Expect(g.lightWin.IsWinType(), "light win flagged");
        Expect(!g.darkPassionateDesire.IsWinType(), "evolved is not win");
    }

    static void Expect(bool condition, string label)
    {
        if (!condition)
        {
            Fail(label);
        }
    }

    static void ExpectEqual(int expected, int actual, string label)
    {
        if (expected != actual)
        {
            Fail(label + " expected " + expected + " got " + actual);
        }
    }

    static void ExpectSame(CombinationRuleSO expected, CombinationRuleSO actual, string label)
    {
        if (!ReferenceEquals(expected, actual))
        {
            Fail(label + " expected " + (expected != null ? expected.name : "null")
                + " got " + (actual != null ? actual.name : "null"));
        }
    }

    static void Fail(string label)
    {
        failures++;
        Console.Error.WriteLine("FAIL: " + label);
    }

    sealed class Graph
    {
        public CombinationRuleSO darkPassion;
        public CombinationRuleSO darkDesire;
        public CombinationRuleSO darkEnvy;
        public CombinationRuleSO darkJoy;
        public CombinationRuleSO darkPassionateDesire;
        public CombinationRuleSO darkEnviousJoy;
        public CombinationRuleSO darkWin;
        public CombinationRuleSO lightPassion;
        public CombinationRuleSO lightDesire;
        public CombinationRuleSO lightEnvy;
        public CombinationRuleSO lightJoy;
        public CombinationRuleSO lightDesiredPassion;
        public CombinationRuleSO lightJoyousEnvy;
        public CombinationRuleSO lightWin;

        public static Graph Build()
        {
            Graph g = new Graph();
            g.darkPassion = Rule("DarkPassion");
            g.darkDesire = Rule("DarkDesire");
            g.darkEnvy = Rule("DarkEnvy");
            g.darkJoy = Rule("DarkJoy");
            g.darkPassionateDesire = Rule("DarkPassionateDesire");
            g.darkEnviousJoy = Rule("DarkEnviousJoy");
            g.darkWin = Rule("DarkWin");
            g.lightPassion = Rule("LightPassion");
            g.lightDesire = Rule("LightDesire");
            g.lightEnvy = Rule("LightEnvy");
            g.lightJoy = Rule("LightJoy");
            g.lightDesiredPassion = Rule("LightDesiredPassion");
            g.lightJoyousEnvy = Rule("LightJoyousEnvy");
            g.lightWin = Rule("LightWin");

            Link(g.darkPassion, g.darkDesire, g.darkPassionateDesire);
            Link(g.darkEnvy, g.darkJoy, g.darkEnviousJoy);
            Link(g.darkPassionateDesire, g.darkEnviousJoy, g.darkWin);
            Link(g.lightPassion, g.lightDesire, g.lightDesiredPassion);
            Link(g.lightEnvy, g.lightJoy, g.lightJoyousEnvy);
            Link(g.lightDesiredPassion, g.lightJoyousEnvy, g.lightWin);
            return g;
        }

        static CombinationRuleSO Rule(string name)
        {
            return new CombinationRuleSO { name = name };
        }

        static void Link(CombinationRuleSO a, CombinationRuleSO b, CombinationRuleSO result)
        {
            a.matchingType = b;
            a.result = result;
            b.matchingType = a;
            b.result = result;
        }
    }
}
