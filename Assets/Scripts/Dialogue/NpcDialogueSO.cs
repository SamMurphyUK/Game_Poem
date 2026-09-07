using UnityEngine;

// The lines an NPC can say, grouped by the combination stage the game has reached.
[CreateAssetMenu(fileName = "NpcDialogue_", menuName = "Game Poem/NPC Dialogue")]
public class NpcDialogueSO : ScriptableObject
{
    [System.Serializable]
    public class StageLines
    {
        public CombinationStage stage;
        [TextArea] public string[] lines;
    }

    [TextArea] public string[] defaultLines;
    public StageLines[] byStage;

    // Stages with nothing written for them fall back to the default lines rather
    // than leaving the NPC silent.
    public string[] GetLines(CombinationStage stage)
    {
        if (byStage != null)
        {
            foreach (StageLines entry in byStage)
            {
                if (entry != null && entry.stage == stage && entry.lines != null && entry.lines.Length > 0)
                {
                    return entry.lines;
                }
            }
        }

        return defaultLines;
    }
}
