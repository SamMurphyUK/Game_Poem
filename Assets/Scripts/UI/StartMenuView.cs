using UnityEngine;
using UnityEngine.UI;

// Shared LOVE title + "I'm feeling lucky." layout used by the start scene
// and by the overlay that covers make no mistakes when you press Play there.
public static class StartMenuView
{
    public static GameObject Build(Transform parent)
    {
        GameObject canvasObject = new GameObject("StartMenuCanvas");
        canvasObject.transform.SetParent(parent, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject dim = new GameObject("Dim");
        dim.transform.SetParent(canvasObject.transform, false);
        Image dimImage = dim.AddComponent<Image>();
        dimImage.color = Color.black;
        dimImage.raycastTarget = false;
        RectTransform dimRect = dimImage.rectTransform;
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.offsetMin = Vector2.zero;
        dimRect.offsetMax = Vector2.zero;

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

        AddLetter(titleRow.transform, font, "L");
        AddLetter(titleRow.transform, font, "O");
        AddDownTriangle(titleRow.transform);
        AddLetter(titleRow.transform, font, "E");

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

        return canvasObject;
    }

    private static void AddLetter(Transform parent, Font font, string letter)
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
