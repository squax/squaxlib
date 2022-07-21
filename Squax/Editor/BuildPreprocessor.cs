using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class BuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        return;
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

#if !SQUAX_RELEASE_MODE
        var releaseWarning = "Currently in Squax[Design Mode], please trigger release mode from main menu before build.";

        Debug.Log(releaseWarning);
        EditorUtility.DisplayDialog("Build Issue", releaseWarning, "Ok");
#endif
    }
}