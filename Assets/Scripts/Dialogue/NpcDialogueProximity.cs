using UnityEngine;

// Hysteresis so an NPC on the edge of range does not flicker on and off.
public static class NpcDialogueProximity
{
    public static bool IsInRange(float distance, bool currentlySpeaking, float speakRange, float silenceRange)
    {
        if (currentlySpeaking)
        {
            return distance <= silenceRange;
        }

        return distance <= speakRange;
    }
}
