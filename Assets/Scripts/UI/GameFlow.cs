using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Covers make no mistakes with the start menu as soon as Play is pressed, then
// the timed love-count / Escape copy, then a fade back to that same menu.
public class GameFlow : MonoBehaviour
{
    // Set by StartMenuController so a real build does not show the menu twice.
    public static bool SkipOpeningMenu;

    private GameObject startMenu;
    private Text message;
    private Image fade;
    private bool onMenu = true;
    private bool ending;
    private float playElapsed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene != GameFlowMath.PlayScene && scene != "make no mistakes")
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
        if (SkipOpeningMenu)
        {
            SkipOpeningMenu = false;
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    private void Update()
    {
        if (ending)
        {
            return;
        }

        if (onMenu)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                HideMenu();
            }

            return;
        }

        playElapsed += Time.deltaTime;
        if (GameFlowMath.ShouldShowEscapePrompt(playElapsed))
        {
            message.text = GameFlowMath.EscapePromptLine;
            message.enabled = true;
        }
        else if (GameFlowMath.ShouldShowLoveCount(playElapsed))
        {
            message.text = GameFlowMath.LoveCountLine;
            message.enabled = true;
        }
        else
        {
            message.enabled = false;
        }

        if (GameFlowMath.CanEndRun(playElapsed) && Input.GetKeyDown(KeyCode.Escape))
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameFlowMath.PlayScene, LoadSceneMode.Single);
    }

    private void ShowMenu()
    {
        onMenu = true;
        playElapsed = 0f;
        if (message != null)
        {
            message.enabled = false;
        }

        if (startMenu != null)
        {
            startMenu.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void HideMenu()
    {
        onMenu = false;
        if (startMenu != null)
        {
            startMenu.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void BuildOverlay()
    {
        startMenu = StartMenuView.Build(transform);

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

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
