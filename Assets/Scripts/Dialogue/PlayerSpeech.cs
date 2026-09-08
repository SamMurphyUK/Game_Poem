using UnityEngine;

// The line a player triangle says the first time it breaks apart after a combine.
public class PlayerSpeech : MonoBehaviour
{
    [SerializeField] private string firstBreakLine = "I think we should see other triangles";

    private SpeechBubble bubble;
    private bool saidFirstBreak;

    private void Awake()
    {
        bubble = GetComponent<SpeechBubble>();
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
    }
}
