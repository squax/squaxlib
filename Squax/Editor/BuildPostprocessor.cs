using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

class BuildPostprocessor : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPostprocessBuild(BuildReport report)
    {
        return;
        // Find valid Scene paths and make a list of EditorBuildSettingsScene
        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();

        List<string> sceneAssets = new List<string>();

        sceneAssets.Add("Assets/Core/Scenes/Preload.unity");
        sceneAssets.Add("Assets/Core/Scenes/Core.unity");
        sceneAssets.Add("Assets/Core/Scenes/Gameplay.unity");

        foreach (var sceneAsset in sceneAssets)
        {
            editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(sceneAsset, true));
        }

        // Set the Build Settings window Scene list
        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        string[] versionParts = PlayerSettings.bundleVersion.Split('.');
        if (versionParts.Length != 3)
        {
            Debug.LogError("BuildPostprocessor failed to update version " + PlayerSettings.bundleVersion);
            return;
        }

        // major-minor-build
        versionParts[2] = (Int32.Parse(versionParts[2]) + 1).ToString();
        PlayerSettings.bundleVersion = String.Join(".", versionParts);

        PlayerSettings.Android.bundleVersionCode = PlayerSettings.Android.bundleVersionCode + 1;
    }
}