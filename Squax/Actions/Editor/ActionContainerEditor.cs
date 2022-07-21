#if !SQUAX_RELEASE_MODE

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Squax.Actions
{
    [CustomEditor(typeof(ActionContainer))]
    public class ActionContainerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if(GUILayout.Button("Open Action Editor"))
            {
                ActionContainerGraphEditor.Open(new SerializedObject(target as ActionContainer));
            }
        }
    }
}

#endif
