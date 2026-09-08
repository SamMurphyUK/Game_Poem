using UnityEngine;

// Speaks a bark above this NPC while a player is close enough, using the lines
// written for the current combination stage. One NPC talking puts every NPC on
// cooldown, for either player.
public class NpcDialogue : MonoBehaviour
{
    [Header("Lines")]
    [SerializeField] private NpcDialogueSO dialogue;
    [SerializeField] private bool silentWhileCombining = true;

    [Header("Proximity")]
    [SerializeField] private float speakRange = 16f;
    [SerializeField] private float silenceRange = 22f;

    private const float RetryPlayerLookupAfter = 0.25f;

    private Transform listener;
    private CombinerComponent ownCombiner;
    private SpeechBubble bubble;
    private float lookupTimer;
    private bool inRange;
    private int lastLineIndex = -1;

    private void Awake()
    {
        ownCombiner = GetComponent<CombinerComponent>();
        bubble = GetComponent<SpeechBubble>();
    }

    private void Update()
    {
        if (dialogue == null || bubble == null)
        {
            return;
        }

        FindListener(Time.deltaTime);

        bool combining = silentWhileCombining && ownCombiner != null && ownCombiner.IsBusy();

        if (combining)
        {
            inRange = false;
            bubble.Hide();
            return;
        }

        float distance = listener != null
            ? Vector2.Distance(transform.position, listener.position)
            : float.PositiveInfinity;
        inRange = NpcDialogueProximity.IsInRange(distance, inRange, speakRange, silenceRange);

        if (bubble.IsShowing || !inRange)
        {
            return;
        }

        if (!DialogueClock.CanNpcSpeak(Time.time))
        {
            return;
        }

        CombinationStage stage = DialogueClock.HighestPlayerStage();
        string[] lines = dialogue.GetLines(stage);
        string line = PickLine(lines);
        if (string.IsNullOrEmpty(line))
        {
            return;
        }

        bubble.Speak(line);
        DialogueClock.MarkNpcSpoke(Time.time, stage);
    }

    private string PickLine(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, lines.Length);
        if (lines.Length > 1 && index == lastLineIndex)
        {
            index = (index + 1) % lines.Length;
        }

        lastLineIndex = index;
        return lines[index];
    }

    private void FindListener(float deltaTime)
    {
        lookupTimer -= deltaTime;
        if (lookupTimer > 0f && listener != null)
        {
            return;
        }

        lookupTimer = RetryPlayerLookupAfter;

        PlayerController[] players = DialogueClock.Players(Time.time);
        Transform nearest = null;
        float best = float.PositiveInfinity;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];
            if (player == null)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < best)
            {
                best = distance;
                nearest = player.transform;
            }
        }

        listener = nearest;
    }
}
