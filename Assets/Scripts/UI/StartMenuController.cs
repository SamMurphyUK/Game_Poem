using UnityEngine;
using UnityEngine.SceneManagement;

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

        StartMenuView.Build(transform);
    }

    private void Update()
    {
        if (starting)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            starting = true;
            GameFlow.SkipOpeningMenu = true;
            SceneManager.LoadScene(GameFlowMath.PlayScene, LoadSceneMode.Single);
        }
    }
}
