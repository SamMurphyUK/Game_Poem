using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpeningCutsceneManager : MonoBehaviour
{
    [Header("Cutscene Characters")]
    [SerializeField] private Transform cutscenePlayer1;
    [SerializeField] private Transform cutscenePlayer2;
    [SerializeField] private CutscenePlayerController cutscenePlayer1Controller;
    [SerializeField] private CutscenePlayerController cutscenePlayer2Controller;
    
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    
    [Header("UI")]
    [SerializeField] private Text instructionText;
    [SerializeField] private Image vignetteImage;
    
    [Header("Timing")]
    [SerializeField] private float instructionDisplayTime = 3f;
    [SerializeField] private float mergeDetectionDistance = 1f;
    [SerializeField] private float mergeAnimationDuration = 1.5f;
    [SerializeField] private float splitAnimationDuration = 1.5f;
    [SerializeField] private float floatInDuration = 2f;
    [SerializeField] private float fadeToBlackDuration = 1.5f;
    
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "make no mistakes";
    
    [Header("Effects")]
    [SerializeField] private Color blackVignetteColor = new Color(0, 0, 0, 0.7f);
    
    private bool hasMerged = false;
    private Vector3 mergePoint;
    private bool cutsceneComplete = false;
    private Image fadeImage;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        CreateFadeImage();
        StartCoroutine(RunOpeningCutscene());
    }

    private void CreateFadeImage()
    {
        // Create a new Canvas for fade overlay
        GameObject fadeCanvasObj = new GameObject("FadeCanvas");
        Canvas fadeCanvas = fadeCanvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 100;
        
        // Create fade image
        GameObject fadeImageObj = new GameObject("FadeImage");
        fadeImageObj.transform.SetParent(fadeCanvasObj.transform);
        fadeImage = fadeImageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        // Set image to fill screen
        RectTransform rectTransform = fadeImageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private IEnumerator RunOpeningCutscene()
    {
        // Phase 1: Show instruction text
        yield return StartCoroutine(ShowInstructions());

        // Phase 2: Enable only cutscene player 1 (WASD) to move
        EnableCutscenePlayerControl(cutscenePlayer1Controller);

        // Phase 3: Wait for player to move and combine
        yield return StartCoroutine(WaitForPlayerMerge());

        // Phase 4: Disable player controls during merge animation
        DisableCutscenePlayerControl(cutscenePlayer1Controller);

        // Phase 5: Animate merge with black vignette focus
        yield return StartCoroutine(AnimateMerge());

        // Phase 6: Keep merged state for a moment
        yield return new WaitForSeconds(0.5f);

        // Phase 7: Split and float away
        yield return StartCoroutine(AnimateSplitAndFloatAway());

        // Phase 8: Float up and out
        yield return StartCoroutine(FloatUpAndOut());

        // Phase 9: Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Phase 10: Load the next scene
        yield return StartCoroutine(LoadNextScene());

        cutsceneComplete = true;
    }

    private IEnumerator ShowInstructions()
    {
        if (instructionText != null)
        {
            instructionText.text = "Move and combine with your partner!";
            instructionText.enabled = true;
            yield return new WaitForSeconds(instructionDisplayTime);
            instructionText.enabled = false;
        }
        else
        {
            yield return new WaitForSeconds(instructionDisplayTime);
        }
    }

    private IEnumerator WaitForPlayerMerge()
    {
        while (!hasMerged)
        {
            float distance = Vector3.Distance(cutscenePlayer1.position, cutscenePlayer2.position);
            
            if (distance < mergeDetectionDistance)
            {
                hasMerged = true;
                mergePoint = (cutscenePlayer1.position + cutscenePlayer2.position) / 2f;
            }

            yield return null;
        }
    }

    private IEnumerator AnimateMerge()
    {
        // Create vignette effect (black focus area)
        CreateVignette();

        float elapsedTime = 0f;

        // Move both players toward merge point while zooming camera
        while (elapsedTime < mergeAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / mergeAnimationDuration;

            // Move players toward merge point
            cutscenePlayer1.position = Vector3.Lerp(cutscenePlayer1.position, mergePoint, progress);
            cutscenePlayer2.position = Vector3.Lerp(cutscenePlayer2.position, mergePoint, progress);

            // Zoom camera to merge point
            Vector3 targetCameraPos = mergePoint + new Vector3(0, 0, -10);
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetCameraPos,
                progress
            );

            // Zoom in on merge point
            float targetZoom = 5f;
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, progress);

            // Intensify vignette
            UpdateVignette(Mathf.Lerp(0, 1, progress));

            yield return null;
        }

        // Ensure final positions
        cutscenePlayer1.position = mergePoint;
        cutscenePlayer2.position = mergePoint;
        mainCamera.transform.position = mergePoint + new Vector3(0, 0, -10);
        mainCamera.orthographicSize = 5f;
        UpdateVignette(1f);
    }

    private IEnumerator AnimateSplitAndFloatAway()
    {
        float elapsedTime = 0f;

        Vector3 player1StartPos = cutscenePlayer1.position;
        Vector3 player2StartPos = cutscenePlayer2.position;

        Vector3 player1EndPos = player1StartPos + new Vector3(-8, 0, 0);
        Vector3 player2EndPos = player2StartPos + new Vector3(8, 0, 0);

        while (elapsedTime < splitAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / splitAnimationDuration;

            // Split players apart
            cutscenePlayer1.position = Vector3.Lerp(player1StartPos, player1EndPos, progress);
            cutscenePlayer2.position = Vector3.Lerp(player2StartPos, player2EndPos, progress);

            // Fade out vignette
            UpdateVignette(Mathf.Lerp(1, 0, progress));

            yield return null;
        }

        cutscenePlayer1.position = player1EndPos;
        cutscenePlayer2.position = player2EndPos;
        UpdateVignette(0f);
    }

    private IEnumerator FloatUpAndOut()
    {
        float elapsedTime = 0f;

        Vector3 player1StartPos = cutscenePlayer1.position;
        Vector3 player2StartPos = cutscenePlayer2.position;

        Vector3 player1EndPos = player1StartPos + new Vector3(0, 15, 0);
        Vector3 player2EndPos = player2StartPos + new Vector3(0, 15, 0);

        while (elapsedTime < floatInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / floatInDuration;

            // Float characters up and out
            cutscenePlayer1.position = Vector3.Lerp(player1StartPos, player1EndPos, progress);
            cutscenePlayer2.position = Vector3.Lerp(player2StartPos, player2EndPos, progress);

            yield return null;
        }

        // Disable cutscene players
        if (cutscenePlayer1 != null)
            cutscenePlayer1.gameObject.SetActive(false);
        if (cutscenePlayer2 != null)
            cutscenePlayer2.gameObject.SetActive(false);
    }

    private IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeToBlackDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeToBlackDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);
    }

    private IEnumerator LoadNextScene()
    {
        // Load the next scene additively (or use LoadScene to replace)
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        yield return null;
    }

    private void CreateVignette()
    {
        if (vignetteImage == null)
        {
            GameObject vignetteCanvasObj = new GameObject("VignetteCanvas");
            Canvas vignetteCanvas = vignetteCanvasObj.AddComponent<Canvas>();
            vignetteCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            vignetteCanvas.sortingOrder = 99;

            GameObject vignetteImageObj = new GameObject("VignetteImage");
            vignetteImageObj.transform.SetParent(vignetteCanvasObj.transform);
            vignetteImage = vignetteImageObj.AddComponent<Image>();

            RectTransform rectTransform = vignetteImageObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        vignetteImage.color = new Color(0, 0, 0, 0);
    }

    private void UpdateVignette(float intensity)
    {
        if (vignetteImage != null)
        {
            vignetteImage.color = new Color(
                blackVignetteColor.r,
                blackVignetteColor.g,
                blackVignetteColor.b,
                blackVignetteColor.a * intensity
            );
        }
    }

    private void EnableCutscenePlayerControl(CutscenePlayerController controller)
    {
        if (controller != null)
            controller.enabled = true;
    }

    private void DisableCutscenePlayerControl(CutscenePlayerController controller)
    {
        if (controller != null)
            controller.enabled = false;
    }

    public bool IsCutsceneComplete => cutsceneComplete;
}