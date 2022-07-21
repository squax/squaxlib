using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if !SQUAX_RELEASE_MODE
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Squax.Actions
{
    [Action("Create/Conditional/Platform Target")]
    public class PlatformTargetConditionalAction : Action
    {
        [SerializeField]
        private RuntimePlatform platform;

        protected override void OnStart()
        {
        }

        protected override void OnUpdate()
        {
            CurrentState = State.EndNextFrame;
        }

        protected override List<Action> OnEnd()
        {
            if(Application.platform == platform)
            {
                return outNodes;
            }
            else
            {
                return new List<Action>();
            }
        }

#if !SQUAX_RELEASE_MODE
        /// <summary>
        /// Title used in tool.
        /// </summary>
        public override string Title
        {
            get
            {
                return "Target Platform";
            }
        }

        public override Vector2 Dimensions
        {
            get
            {
                return new Vector2(140, 60);
            }
        }

        override protected void OnWindowGUI(int windowID)
        {
            EditorGUILayout.LabelField(platform + "", "");
        }

        override protected void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("platform"), new GUIContent("Target Platform"));
        }
#endif
    }
}
