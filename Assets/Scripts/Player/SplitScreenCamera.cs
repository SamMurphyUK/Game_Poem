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
    [SerializeField] private float rejoinBuffer = 3f;
    [SerializeField] private Vector3 followOffset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float padding = 2f;
    [SerializeField] private float minimumSize = 0f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float dividerWidth = 0.01f;
    [SerializeField] private bool allowSharedCamera = false;

    private bool isSplitScreen = false;
    private Image fadeImage;
    private Image dividerImage;
    private bool isTransitioning = false;
    private Transform leftPlayer;
    private Transform rightPlayer;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (splitCamera == null)
        {
            Debug.LogError("Split screen camera not assigned!");
            return;
        }

        CreateFadeImage();
        CreateDividerLine();

        // Do not rebind backgroundCanvas.worldCamera. That canvas still has
        // leftover walls as Screen Space children, which then ride whichever
        // camera it points at.
        _ = backgroundCanvas;

        if (player1 == null || player2 == null)
        {
            Debug.LogError("Split screen players are not assigned!");
            return;
        }

        AssignViewPlayers();

        // Shared-camera / midpoint follow is what made WASD drag the other
        // half of the level. Stay split unless a scene explicitly opts in.
        isSplitScreen = !SplitScreenFollow.ShouldShareOneCamera(
            allowSharedCamera,
            false,
            Vector3.Distance(player1.position, player2.position),
            splitDistanceThreshold,
            rejoinBuffer);
        SetupCameras();
    }

    private void AssignViewPlayers()
    {
        SplitScreenFollow.AssignSides(player1.position.x, player2.position.x, out bool player1OnLeft);
        if (player1OnLeft)
        {
            leftPlayer = player1;
            rightPlayer = player2;
        }
        else
        {
            leftPlayer = player2;
            rightPlayer = player1;
        }
    }

    private void CreateFadeImage()
    {
        GameObject fadeCanvasObj = new GameObject("FadeCanvas");
        Canvas fadeCanvas = fadeCanvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        GameObject fadeImageObj = new GameObject("FadeImage");
        fadeImageObj.transform.SetParent(fadeCanvasObj.transform);
        fadeImage = fadeImageObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        
        RectTransform rectTransform = fadeImageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        fadeCanvas.sortingOrder = 100;
    }

    private void CreateDividerLine()
    {
        GameObject dividerCanvasObj = new GameObject("DividerCanvas");
        Canvas dividerCanvas = dividerCanvasObj.AddComponent<Canvas>();
        dividerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dividerCanvas.sortingOrder = 50;
        
        GameObject dividerImageObj = new GameObject("DividerLine");
        dividerImageObj.transform.SetParent(dividerCanvasObj.transform);
        dividerImage = dividerImageObj.AddComponent<Image>();
        dividerImage.color = new Color(0, 0, 0, 1);
        
        RectTransform rectTransform = dividerImageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.offsetMin = new Vector2(-dividerWidth * Screen.width / 2f, 0);
        rectTransform.offsetMax = new Vector2(dividerWidth * Screen.width / 2f, 0);
        
        dividerImage.enabled = false;
    }

    private void LateUpdate()
    {
        if (player1 == null || player2 == null || leftPlayer == null || rightPlayer == null)
            return;

        if (allowSharedCamera && !isTransitioning)
        {
            float distanceBetweenPlayers = Vector3.Distance(player1.position, player2.position);
            bool shouldShare = SplitScreenFollow.ShouldShareOneCamera(
                allowSharedCamera,
                isSplitScreen,
                distanceBetweenPlayers,
                splitDistanceThreshold,
                rejoinBuffer);
            bool shouldBeSplitScreen = !shouldShare;

            if (shouldBeSplitScreen != isSplitScreen)
            {
                isSplitScreen = shouldBeSplitScreen;
                StartCoroutine(TransitionCameras());
            }
        }

        if (isSplitScreen)
            UpdateSplitScreen();
        else
            UpdateSingleCamera();
    }

    private IEnumerator TransitionCameras()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeToBlack(fadeDuration));
        SetupCameras();
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
            mainCamera.gameObject.SetActive(true);
            splitCamera.gameObject.SetActive(true);

            mainCamera.rect = new Rect(0, 0, 0.5f, 1);
            splitCamera.rect = new Rect(0.5f, 0, 0.5f, 1);

            splitCamera.orthographic = mainCamera.orthographic;
            splitCamera.orthographicSize = mainCamera.orthographicSize;

            mainCamera.transform.position = SplitScreenFollow.FollowPosition(leftPlayer.position, followOffset);
            splitCamera.transform.position = SplitScreenFollow.FollowPosition(rightPlayer.position, followOffset);

            dividerImage.enabled = true;
        }
        else
        {
            mainCamera.rect = new Rect(0, 0, 1, 1);
            splitCamera.gameObject.SetActive(false);

            Vector3 midpoint = (player1.position + player2.position) / 2f;
            mainCamera.transform.position = SplitScreenFollow.FollowPosition(midpoint, followOffset);

            dividerImage.enabled = false;
        }
    }

    private void UpdateSingleCamera()
    {
        Vector3 midpoint = (player1.position + player2.position) / 2f;
        Vector3 targetPosition = SplitScreenFollow.FollowPosition(midpoint, followOffset);

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        float distanceBetweenPlayers = Vector3.Distance(player1.position, player2.position);
        float requiredSize = Mathf.Max(minimumSize, (distanceBetweenPlayers / 2f) + padding);
        mainCamera.orthographicSize = Mathf.Lerp(
            mainCamera.orthographicSize,
            requiredSize,
            smoothSpeed * Time.deltaTime
        );
    }

    private void UpdateSplitScreen()
    {
        // Left viewport stays on the world-left player, right on the world-right
        // player. Walking one of them must not translate the other half.
        if (mainCamera != null && leftPlayer != null)
        {
            mainCamera.transform.position = SplitScreenFollow.FollowPosition(leftPlayer.position, followOffset);
        }

        if (splitCamera != null && rightPlayer != null)
        {
            splitCamera.transform.position = SplitScreenFollow.FollowPosition(rightPlayer.position, followOffset);
        }
    }
}
