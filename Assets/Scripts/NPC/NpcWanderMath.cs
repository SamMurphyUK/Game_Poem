using UnityEngine;

// Destination and bounce helpers for wandering NPCs. Kept off MonoBehaviour
// so the leash and wall-turn rules can be checked without the editor.
public static class NpcWanderMath
{
    public static Vector2 Bounce(Vector2 incoming, Vector2 normal)
    {
        if (normal.sqrMagnitude < 0.0001f)
        {
            return incoming.sqrMagnitude > 0.0001f ? -incoming.normalized : Vector2.up;
        }

        Vector2 reflected = Vector2.Reflect(incoming, normal.normalized);
        if (reflected.sqrMagnitude < 0.0001f)
        {
            return -incoming.normalized;
        }

        return reflected.normalized;
    }

    public static Vector2 Steer(Vector2 position, Vector2 home, float radius, Vector2 desired)
    {
        if (radius <= 0f)
        {
            return desired.sqrMagnitude > 0.0001f ? desired.normalized : desired;
        }

        Vector2 fromHome = position - home;
        if (fromHome.sqrMagnitude > radius * radius)
        {
            return (home - position).normalized;
        }

        return desired.sqrMagnitude > 0.0001f ? desired.normalized : desired;
    }

    public static bool IsOutsideRadius(Vector2 position, Vector2 home, float radius)
    {
        if (radius <= 0f)
        {
            return false;
        }

        return (position - home).sqrMagnitude > radius * radius;
    }
}
