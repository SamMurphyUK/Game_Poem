using UnityEngine;
using UnityEngine.UI;

// World-space line that follows this transform for a few seconds, then fades.
public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private Font font;
    [SerializeField] private float heightAboveSprite = 0.8f;
    [SerializeField] private float labelWidth = 8f;
    [SerializeField] private float textSize = 0.55f;
    [SerializeField] private Color textColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private int sortingOrderOffset = 10;
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float lingerSeconds = 4f;

    private const float PixelsPerUnit = 100f;
    private const float LabelZ = -0.2f;

    private SpriteRenderer body;
    private GameObject labelRoot;
    private Text label;
    private float visibility;
    private float lingerLeft;
    private bool showing;

    public bool IsShowing => showing;

    private void Awake()
    {
        body = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        if (labelRoot != null)
        {
            Destroy(labelRoot);
        }
    }

    public void Speak(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return;
        }

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
        lingerLeft = lingerSeconds;
        showing = true;
    }

    public void Hide()
    {
        showing = false;
        lingerLeft = 0f;
    }

    private void Update()
    {
        if (showing)
        {
            lingerLeft -= Time.deltaTime;
            if (lingerLeft <= 0f)
            {
                showing = false;
            }
        }

        Fade();
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

    private void Fade()
    {
        if (label == null)
        {
            return;
        }

        float step = fadeDuration > 0f ? Time.deltaTime / fadeDuration : 1f;
        visibility = Mathf.MoveTowards(visibility, showing ? 1f : 0f, step);

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
            Debug.LogWarning("No font available, speech on " + name + " cannot be drawn");
            return;
        }

        labelRoot = new GameObject(name + " Speech", typeof(RectTransform), typeof(Canvas));
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
