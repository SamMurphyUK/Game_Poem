#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Builds the playable game only (make no mistakes). TEST and StartMenu stay
// out of the HTML output — GameFlow already covers the title on Play.
public static class LoveBetaWebGLBuild
{
    public const string ProductName = "Love (BETA)";
    public const string OutputFolder = "Builds/WebGL/Love (BETA)";
    public const string PlayScene = "Assets/Scenes/make no mistakes.unity";

    [MenuItem("Love (BETA)/Apply WebGL Player Settings")]
    public static void ApplySettings()
    {
        PlayerSettings.productName = ProductName;
        PlayerSettings.companyName = "Love";
        PlayerSettings.bundleVersion = "0.1.0-beta";
        PlayerSettings.runInBackground = true;
        PlayerSettings.defaultWebScreenWidth = 1920;
        PlayerSettings.defaultWebScreenHeight = 1080;
        PlayerSettings.WebGL.template = "PROJECT:LoveBeta";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.initialMemorySize = 256;
    }

    [MenuItem("Love (BETA)/Build WebGL")]
    public static void Build()
    {
        ApplySettings();

        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.WebGL, BuildTarget.WebGL))
        {
            Debug.LogError("Love (BETA): could not switch the active build target to WebGL.");
            return;
        }

        Directory.CreateDirectory(OutputFolder);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { PlayScene },
            locationPathName = OutputFolder,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Love (BETA) WebGL build finished: " + Path.GetFullPath(OutputFolder));
            EditorUtility.RevealInFinder(OutputFolder);
        }
        else
        {
            Debug.LogError("Love (BETA) WebGL build failed: " + report.summary.result);
        }
    }
}
#endif
