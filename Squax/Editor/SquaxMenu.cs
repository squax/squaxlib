using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SquaxMenu : Editor
{
#if !SQUAX_RELEASE_MODE
    [MenuItem("Squax[Design Mode]/Enable Release Mode")]
    static void EnableReleaseMode()
    {
        UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Standalone, "SQUAX_RELEASE_MODE");
        UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Android, "SQUAX_RELEASE_MODE");
    }
#endif

#if SQUAX_RELEASE_MODE
    [MenuItem("Squax[Release Mode]/Enable Design Mode")]
    static void DisableReleaseMode()
    {
        UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Standalone, "");
        UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Android, "");
    }
#endif
}
