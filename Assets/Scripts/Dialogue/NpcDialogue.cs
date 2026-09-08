using UnityEngine;
using UnityEngine.UI;

// Speaks a line above this object while any player is close enough to hear it,
// cycling through whichever lines are written for the combination stage the
// game has reached. The label is built at runtime.
public class NpcDialogue : MonoBehaviour
{
    [Header("Lines")]
    [SerializeField] private NpcDialogueSO dialogue;
    [SerializeField] private float secondsPerLine = 3.5f;
    [SerializeField] private bool silentWhileCombining = true;

    [Header("Proximity")]
    [SerializeField] private float speakRange = 16f;
    [SerializeField] private float silenceRange = 22f;

    [Header("Label")]
    [SerializeField] private Font font;
    [SerializeField] private float heightAboveSprite = 0.8f;
    [SerializeField] private float labelWidth = 8f;
    [SerializeField] private float textSize = 0.55f;
    [SerializeField] private Color textColor = new Color(0.93f, 0.9f, 0.86f, 1f);
    [SerializeField] private int sortingOrderOffset = 10;
    [SerializeField] private float fadeDuration = 0.35f;

    private const float PixelsPerUnit = 100f;
    private const float RetryPlayerLookupAfter = 0.25f;
    private const float LabelZ = -0.2f;

    private Transform listener;
    private CombinerComponent stageSource;
    private CombinerComponent ownCombiner;
    private SpriteRenderer body;

    private GameObject labelRoot;
    private Text label;

    private string[] lines;
    private CombinationStage shownStage;
    private int lineIndex;
    private float lineTimer;
    private float lookupTimer;
    private float visibility;
    private bool speaking;

    private void Awake()
    {
        ownCombiner = GetComponent<CombinerComponent>();
        body = GetComponent<SpriteRenderer>();
        shownStage = CombinationStage.Stage1;
        lines = dialogue != null ? dialogue.GetLines(shownStage) : null;
    }

    private void OnDestroy()
    {
        if (labelRoot != null)
        {
            Destroy(labelRoot);
        }
    }

    private void Update()
    {
        if (dialogue == null)
        {
            return;
        }

        float deltaTime = Time.deltaTime;

        FindListener(deltaTime);
        FollowGameState();

        speaking = HasLines() && WithinRange();

        if (speaking)
        {
            AdvanceLine(deltaTime);
        }

        Fade(deltaTime);
    }

    private void LateUpdate()
    {
        if (labelRoot == null || !labelRoot.activeSelf)
        {
            return;
        }

        float top = body != null ? body.bounds.max.y : transform.position.y;
        labelRoot.transform.position = new Vector3(transform.position.x, top + heightAboveSprite, LabelZ);
        labelRoot.transform.rotation = Quaternion.identity;
    }

    private void FindListener(float deltaTime)
    {
        lookupTimer -= deltaTime;
        if (lookupTimer > 0f && listener != null)
        {
            return;
        }

        lookupTimer = RetryPlayerLookupAfter;

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Transform nearest = null;
        CombinerComponent nearestCombiner = null;
        CombinerComponent highest = null;
        float best = float.PositiveInfinity;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player = players[i];
            if (player == null)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, player.transform.position);
            CombinerComponent combiner = player.GetComponent<CombinerComponent>();

            if (distance < best)
            {
                best = distance;
                nearest = player.transform;
                nearestCombiner = combiner;
            }

            if (combiner != null && (highest == null || combiner.GetCurrentStage() > highest.GetCurrentStage()))
            {
                highest = combiner;
            }
        }

        listener = nearest;
        stageSource = highest != null ? highest : nearestCombiner;
    }

    // A combination anywhere in the level changes what nearby NPCs say.
    private CombinationStage CurrentStage()
    {
        return stageSource != null ? stageSource.GetCurrentStage() : CombinationStage.Stage1;
    }

    private void FollowGameState()
    {
        CombinationStage stage = CurrentStage();

        if (stage == shownStage && lines != null)
        {
            return;
        }

        shownStage = stage;
        lines = dialogue.GetLines(stage);
        lineIndex = 0;
        lineTimer = 0f;

        if (HasLines() && labelRoot != null && labelRoot.activeSelf)
        {
            Show(lines[0]);
        }
    }

    private bool HasLines()
    {
        return lines != null && lines.Length > 0;
    }

    private bool WithinRange()
    {
        if (listener == null)
        {
            return false;
        }

        if (silentWhileCombining && ownCombiner != null && ownCombiner.IsMovingToCombine())
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, listener.position);
        return NpcDialogueProximity.IsInRange(distance, speaking, speakRange, silenceRange);
    }

    private void AdvanceLine(float deltaTime)
    {
        if (labelRoot == null || !labelRoot.activeSelf)
        {
            lineIndex = 0;
            lineTimer = 0f;
            Show(lines[0]);
            return;
        }

        lineTimer += deltaTime;

        if (lineTimer < secondsPerLine)
        {
            return;
        }

        lineTimer -= secondsPerLine;
        lineIndex = (lineIndex + 1) % lines.Length;
        Show(lines[lineIndex]);
    }

    private void Show(string line)
    {
        if (label == null)
        {
            BuildLabel();
        }

        if (label == null)
        {
            return;
        }

        label.text = line;
        labelRoot.SetActive(true);
    }

    private void Fade(float deltaTime)
    {
        if (label == null)
        {
            return;
        }

        float step = fadeDuration > 0f ? deltaTime / fadeDuration : 1f;
        visibility = Mathf.MoveTowards(visibility, speaking ? 1f : 0f, step);

        Color color = textColor;
        color.a *= visibility;
        label.color = color;

        if (visibility <= 0f && labelRoot.activeSelf)
        {
            labelRoot.SetActive(false);
        }
    }

    private void BuildLabel()
    {
        Font drawWith = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (drawWith == null)
        {
            drawWith = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        if (drawWith == null)
        {
            Debug.LogWarning("No font available, dialogue on " + name + " cannot be drawn");
            return;
        }

        // Keep the label in world space so NPC scale and wander rotation cannot
        // squash or spin the text, and sit it closer to the camera than the walls.
        labelRoot = new GameObject(name + " Dialogue", typeof(RectTransform), typeof(Canvas));
        labelRoot.transform.SetParent(null, worldPositionStays: true);

        RectTransform rootRect = (RectTransform)labelRoot.transform;
        rootRect.pivot = new Vector2(0.5f, 0f);
        rootRect.sizeDelta = new Vector2(labelWidth * PixelsPerUnit, textSize * 2f * PixelsPerUnit);
        rootRect.localScale = new Vector3(1f / PixelsPerUnit, 1f / PixelsPerUnit, 1f / PixelsPerUnit);

        Canvas canvas = labelRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = sortingOrderOffset;

        if (body != null)
        {
            canvas.sortingLayerID = body.sortingLayerID;
            canvas.sortingOrder = body.sortingOrder + sortingOrderOffset;
        }

        GameObject lineObject = new GameObject("Line", typeof(RectTransform), typeof(Text));
        lineObject.transform.SetParent(rootRect, worldPositionStays: false);

        label = lineObject.GetComponent<Text>();
        label.font = drawWith;
        label.fontSize = Mathf.RoundToInt(textSize * PixelsPerUnit);
        label.alignment = TextAnchor.LowerCenter;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        label.color = textColor;

        RectTransform lineRect = label.rectTransform;
        lineRect.anchorMin = Vector2.zero;
        lineRect.anchorMax = Vector2.one;
        lineRect.offsetMin = Vector2.zero;
        lineRect.offsetMax = Vector2.zero;

        labelRoot.SetActive(false);
    }
}
