using UnityEngine;

// The first-break line, plus rare idle barks that follow the combination stage.
public class PlayerSpeech : MonoBehaviour
{
    [SerializeField] private string firstBreakLine = "I think we should see other triangles";

    private SpeechBubble bubble;
    private CombinerComponent combiner;
    private bool saidFirstBreak;
    private int lastIdleIndex = -1;

    private void Awake()
    {
        bubble = GetComponent<SpeechBubble>();
        combiner = GetComponent<CombinerComponent>();
    }

    private void Start()
    {
        DialogueClock.EnsureFirstPlayerWait(Time.time, PlayerSpeechMath.NextWaitSeconds(Random.value));
    }

    private void Update()
    {
        if (!CanIdleSpeak())
        {
            return;
        }

        if (!DialogueClock.CanPlayerSpeak(Time.time))
        {
            return;
        }

        // Two players may both be ready; a coin flip keeps one from always winning.
        if (Random.value >= 0.5f)
        {
            return;
        }

        CombinationStage stage = combiner != null
            ? combiner.GetCurrentStage()
            : CombinationStage.Stage1;
        string[] lines = PlayerSpeechMath.LinesFor((int)stage);
        int index = PlayerSpeechMath.PickLineIndex(lines.Length, Random.value, lastIdleIndex);
        if (index < 0)
        {
            return;
        }

        if (!DialogueClock.TryClaimPlayerSpeak(Time.time, PlayerSpeechMath.NextWaitSeconds(Random.value)))
        {
            return;
        }

        lastIdleIndex = index;
        bubble.Speak(lines[index]);
    }

    public void SayFirstBreak()
    {
        if (saidFirstBreak || string.IsNullOrEmpty(firstBreakLine))
        {
            return;
        }

        if (bubble == null)
        {
            bubble = GetComponent<SpeechBubble>();
        }

        if (bubble == null)
        {
            return;
        }

        saidFirstBreak = true;
        bubble.Speak(firstBreakLine);
        DialogueClock.MarkPlayerSpoke(Time.time, PlayerSpeechMath.NextWaitSeconds(Random.value));
    }

    private bool CanIdleSpeak()
    {
        if (bubble == null)
        {
            bubble = GetComponent<SpeechBubble>();
        }

        if (bubble == null || bubble.IsShowing)
        {
            return false;
        }

        return combiner == null || !combiner.IsBusy();
    }
}
