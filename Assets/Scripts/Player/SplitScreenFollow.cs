using UnityEngine;

// Pure layout helpers for the two-player cameras. Kept off MonoBehaviour so
// the side assignment and follow positions can be checked without the editor.
public static class SplitScreenFollow
{
    public static void AssignSides(
        float player1X,
        float player2X,
        out bool player1OnLeft)
    {
        player1OnLeft = player1X <= player2X;
    }

    public static Vector3 FollowPosition(Vector3 playerPosition, Vector3 offset)
    {
        return playerPosition + offset;
    }

    public static bool ShouldShareOneCamera(
        bool allowSharedCamera,
        bool currentlySplit,
        float distance,
        float splitDistanceThreshold,
        float rejoinBuffer)
    {
        if (!allowSharedCamera)
        {
            return false;
        }

        bool shouldBeSplit = currentlySplit
            ? distance > splitDistanceThreshold - rejoinBuffer
            : distance > splitDistanceThreshold;
        return !shouldBeSplit;
    }
}
