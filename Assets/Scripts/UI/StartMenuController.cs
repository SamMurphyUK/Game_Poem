using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Black start screen: LOVE with a downward triangle for the V, and the only
// line of copy, "I'm feeling lucky."
public class StartMenuController : MonoBehaviour
{
    private bool starting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (SceneManager.GetActiveScene().name != GameFlowMath.StartMenuScene)
        {
            return;
        }

        if (Object.FindFirstObjectByType<StartMenuController>() != null)
        {
            return;
        }

        GameObject host = new GameObject("StartMenu");
        host.AddComponent<StartMenuController>();
    }

    private void Start()
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
        }

        BuildMenu();
    }

    private void Update()
    {
        if (starting)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            BeginPlay();
        }
    }

    private void BeginPlay()
    {
        starting = true;
        SceneManager.LoadScene(GameFlowMath.PlayScene, LoadSceneMode.Single);
    }

    private void BuildMenu()
    {
        GameObject canvasObject = new GameObject("StartMenuCanvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        GameObject titleRow = new GameObject("Title");
        titleRow.transform.SetParent(canvasObject.transform, false);
        HorizontalLayoutGroup row = titleRow.AddComponent<HorizontalLayoutGroup>();
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = false;
        row.spacing = 8f;
        RectTransform rowRect = titleRow.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.55f);
        rowRect.anchorMax = new Vector2(0.5f, 0.55f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.sizeDelta = new Vector2(900f, 220f);
        rowRect.anchoredPosition = Vector2.zero;

        AddLetter(titleRow.transform, font, "L", 0f);
        AddLetter(titleRow.transform, font, "O", 0f);
        AddDownTriangle(titleRow.transform);
        AddLetter(titleRow.transform, font, "E", 0f);

        GameObject lucky = new GameObject("Lucky");
        lucky.transform.SetParent(canvasObject.transform, false);
        Text luckyText = lucky.AddComponent<Text>();
        luckyText.font = font;
        luckyText.fontSize = 36;
        luckyText.alignment = TextAnchor.MiddleCenter;
        luckyText.color = Color.white;
        luckyText.text = "I'm feeling lucky.";
        luckyText.raycastTarget = false;
        RectTransform luckyRect = luckyText.rectTransform;
        luckyRect.anchorMin = new Vector2(0.5f, 0.38f);
        luckyRect.anchorMax = new Vector2(0.5f, 0.38f);
        luckyRect.pivot = new Vector2(0.5f, 0.5f);
        luckyRect.sizeDelta = new Vector2(800f, 80f);
        luckyRect.anchoredPosition = Vector2.zero;
    }

    private static void AddLetter(Transform parent, Font font, string letter, float lift)
    {
        GameObject go = new GameObject(letter);
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = 140;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = letter;
        text.raycastTarget = false;
        LayoutElement layout = go.AddComponent<LayoutElement>();
        layout.preferredWidth = 120f;
        layout.preferredHeight = 180f;
        if (lift != 0f)
        {
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, lift);
        }
    }

    private static void AddDownTriangle(Transform parent)
    {
        GameObject go = new GameObject("V");
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.color = Color.white;
        image.sprite = DownTriangleSprite();
        image.raycastTarget = false;
        LayoutElement layout = go.AddComponent<LayoutElement>();
        layout.preferredWidth = 110f;
        layout.preferredHeight = 110f;
        layout.minWidth = 110f;
        layout.minHeight = 110f;
        // Sit the triangle a little above the L O E baseline so it reads as the V.
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.15f);
    }

    private static Sprite downTriangle;

    private static Sprite DownTriangleSprite()
    {
        if (downTriangle != null)
        {
            return downTriangle;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Color clear = new Color(1f, 1f, 1f, 0f);
        Color solid = Color.white;
        for (int y = 0; y < size; y++)
        {
            float t = 1f - (y + 0.5f) / size;
            float half = t * 0.5f;
            float left = 0.5f - half;
            float right = 0.5f + half;
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                texture.SetPixel(x, y, u >= left && u <= right ? solid : clear);
            }
        }

        texture.Apply();
        downTriangle = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        downTriangle.name = "DownTriangle";
        return downTriangle;
    }
}
