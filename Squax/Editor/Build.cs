using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;

// Output the build size or a failure depending on BuildPlayer.

public class Build : MonoBehaviour
{
    [MenuItem("Build/Build and Run Android")]
    public static void BuildAndRunAndroid()
    {
        Preprocess();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        //buildPlayerOptions.scenes = new[] { "Assets/Core/Scenes/Preload.unity" };
        buildPlayerOptions.locationPathName = "Builds/Android.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.ShowBuiltPlayer | BuildOptions.CompressTextures | BuildOptions.AutoRunPlayer;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    private static void Preprocess()
    {
        Debug.Log("Preprocessing Build - removing Core.unity");

        // Find valid Scene paths and make a list of EditorBuildSettingsScene
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();

        List<string> sceneAssets = new List<string>();
        sceneAssets.Add("Assets/Core/Scenes/Preload.unity");
        sceneAssets.Add("Assets/Core/Scenes/Gameplay.unity");

        foreach (var sceneAsset in sceneAssets)
        {
            editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(sceneAsset, true));
        }

        // Set the Build Settings window Scene list
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }

    [MenuItem("Build/Build Android")]
    public static void BuildAndroid()
    {
        Preprocess();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        //buildPlayerOptions.scenes = new[] { "Assets/Core/Scenes/Preload.unity" };
        buildPlayerOptions.locationPathName = "Builds/Android.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.ShowBuiltPlayer | BuildOptions.CompressTextures;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
}