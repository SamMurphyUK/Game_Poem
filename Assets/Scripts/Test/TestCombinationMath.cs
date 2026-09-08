// Layout for the TEST arena: two rows of emotion types beside the players.
public static class TestCombinationMath
{
    public static float SlotX(int index, int count, float spacing)
    {
        if (count <= 1)
        {
            return 0f;
        }

        float start = -0.5f * (count - 1) * spacing;
        return start + index * spacing;
    }
}
