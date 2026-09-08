using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Timed overlay during play: love-count copy at 4 minutes, Escape prompt at
// 4.5, then a fade back to the start menu.
public class GameFlow : MonoBehaviour
{
    private Text message;
    private Image fade;
    private bool ending;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (SceneManager.GetActiveScene().name != GameFlowMath.PlayScene)
        {
            return;
        }

        if (Object.FindFirstObjectByType<GameFlow>() != null)
        {
            return;
        }

        GameObject host = new GameObject("GameFlow");
        host.AddComponent<GameFlow>();
    }

    private void Start()
    {
        BuildOverlay();
    }

    private void Update()
    {
        if (ending)
        {
            return;
        }

        float elapsed = Time.timeSinceLevelLoad;
        if (GameFlowMath.ShouldShowEscapePrompt(elapsed))
        {
            message.text = GameFlowMath.EscapePromptLine;
            message.enabled = true;
        }
        else if (GameFlowMath.ShouldShowLoveCount(elapsed))
        {
            message.text = GameFlowMath.LoveCountLine;
            message.enabled = true;
        }
        else
        {
            message.enabled = false;
        }

        if (GameFlowMath.CanEndRun(elapsed) && Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(FadeToMenu());
        }
    }

    private IEnumerator FadeToMenu()
    {
        ending = true;
        float elapsed = 0f;
        while (elapsed < GameFlowMath.FadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = GameFlowMath.FadeAlpha(elapsed, GameFlowMath.FadeSeconds);
            fade.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fade.color = Color.black;
        SceneManager.LoadScene(GameFlowMath.StartMenuScene, LoadSceneMode.Single);
    }

    private void BuildOverlay()
    {
        GameObject canvasObject = new GameObject("GameFlowCanvas");
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        GameObject textObject = new GameObject("Message");
        textObject.transform.SetParent(canvasObject.transform, false);
        message = textObject.AddComponent<Text>();
        message.font = font;
        message.fontSize = 56;
        message.alignment = TextAnchor.MiddleCenter;
        message.color = Color.white;
        message.horizontalOverflow = HorizontalWrapMode.Wrap;
        message.verticalOverflow = VerticalWrapMode.Overflow;
        message.raycastTarget = false;
        message.enabled = false;
        RectTransform textRect = message.rectTransform;
        textRect.anchorMin = new Vector2(0.08f, 0.35f);
        textRect.anchorMax = new Vector2(0.92f, 0.65f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        GameObject fadeObject = new GameObject("Fade");
        fadeObject.transform.SetParent(canvasObject.transform, false);
        fade = fadeObject.AddComponent<Image>();
        fade.color = new Color(0f, 0f, 0f, 0f);
        fade.raycastTarget = false;
        RectTransform fadeRect = fade.rectTransform;
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
    }
}
