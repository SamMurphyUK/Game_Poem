using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SplitScreenCamera : MonoBehaviour
{
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera splitCamera;
    [SerializeField] private Canvas backgroundCanvas;
    
    [SerializeField] private float splitDistanceThreshold = 15f;
    [SerializeField] private Vector3 followOffset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float padding = 2f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float dividerWidth = 0.01f;

    private bool isSplitScreen = false;
    private Image fadeImage;
    private Image dividerImage;
    private bool isTransitioning = false;
    private Camera originalCanvasCamera;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (splitCamera == null)
        {
            Debug.LogError("Split screen camera not assigned!");
            return;
        }

        // Store the original canvas camera
        if (backgroundCanvas != null)
            originalCanvasCamera = backgroundCanvas.worldCamera;

        // Setup fade image
        CreateFadeImage();
        
        // Setup divider line
        CreateDividerLine();
        
        // Initially disable split camera
        splitCamera.gameObject.SetActive(false);
    }

    private void CreateFadeImage()
    {
        // Create a new Canvas for fade overlay
        GameObject fadeCanvasObj = new GameObject("FadeCanvas");
        Canvas fadeCanvas = fadeCanvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
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
        
        // Set sorting order to be on top
        fadeCanvas.sortingOrder = 100;
    }

    private void CreateDividerLine()
    {
        // Create a new Canvas for divider overlay
        GameObject dividerCanvasObj = new GameObject("DividerCanvas");
        Canvas dividerCanvas = dividerCanvasObj.AddComponent<Canvas>();
        dividerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dividerCanvas.sortingOrder = 50;
        
        // Create divider image
        GameObject dividerImageObj = new GameObject("DividerLine");
        dividerImageObj.transform.SetParent(dividerCanvasObj.transform);
        dividerImage = dividerImageObj.AddComponent<Image>();
        dividerImage.color = new Color(0, 0, 0, 1);
        
        // Set divider to vertical center line
        RectTransform rectTransform = dividerImageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.offsetMin = new Vector2(-dividerWidth * Screen.width / 2f, 0);
        rectTransform.offsetMax = new Vector2(dividerWidth * Screen.width / 2f, 0);
        
        // Initially hide divider
        dividerImage.enabled = false;
    }

    private void Update()
    {
        if (player1 == null || player2 == null || isTransitioning)
            return;

        float distanceBetweenPlayers = Vector3.Distance(player1.position, player2.position);
        bool shouldBeSplitScreen = distanceBetweenPlayers > splitDistanceThreshold;

        if (shouldBeSplitScreen != isSplitScreen)
        {
            isSplitScreen = shouldBeSplitScreen;
            StartCoroutine(TransitionCameras());
        }

        if (isSplitScreen)
            UpdateSplitScreen();
        else
            UpdateSingleCamera();
    }

    private IEnumerator TransitionCameras()
    {
        isTransitioning = true;

        // Fade to black
        yield return StartCoroutine(FadeToBlack(fadeDuration));

        // Switch camera setup
        SetupCameras();

        // Fade from black
        yield return StartCoroutine(FadeFromBlack(fadeDuration));

        isTransitioning = false;
    }

    private IEnumerator FadeToBlack(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);
    }

    private IEnumerator FadeFromBlack(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / duration));
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
    }

    private void SetupCameras()
    {
        if (isSplitScreen)
        {
            // Enable split screen mode
            mainCamera.gameObject.SetActive(true);
            splitCamera.gameObject.SetActive(true);

            // Set viewports for vertical split (left and right)
            mainCamera.rect = new Rect(0, 0, 0.5f, 1);
            splitCamera.rect = new Rect(0.5f, 0, 0.5f, 1);
            
            // Canvas stays on main camera - it will render on its half
            if (backgroundCanvas != null)
                backgroundCanvas.worldCamera = mainCamera;
            
            // Show divider line
            dividerImage.enabled = true;
        }
        else
        {
            // Single camera mode
            mainCamera.rect = new Rect(0, 0, 1, 1);
            splitCamera.gameObject.SetActive(false);
            
            // Canvas back to original camera or main camera
            if (backgroundCanvas != null)
                backgroundCanvas.worldCamera = originalCanvasCamera != null ? originalCanvasCamera : mainCamera;
            
            // Hide divider line
            dividerImage.enabled = false;
        }
    }

    private void UpdateSingleCamera()
    {
        // When close, focus on the midpoint between both players
        Vector3 midpoint = (player1.position + player2.position) / 2f;
        Vector3 targetPosition = midpoint + followOffset;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        // Adjust camera size to keep both players visible
        float distanceBetweenPlayers = Vector3.Distance(player1.position, player2.position);
        float requiredSize = (distanceBetweenPlayers / 2f) + padding;
        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            requiredSize,
            smoothSpeed * Time.deltaTime
        );
    }

    private void UpdateSplitScreen()
    {
        // Follow player1 with main camera
        Vector3 targetPos1 = player1.position + followOffset;
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPos1,
            smoothSpeed * Time.deltaTime
        );

        // Follow player2 with split camera
        Vector3 targetPos2 = player2.position + followOffset;
        splitCamera.transform.position = Vector3.Lerp(
            splitCamera.transform.position,
            targetPos2,
            smoothSpeed * Time.deltaTime
        );
    }
}