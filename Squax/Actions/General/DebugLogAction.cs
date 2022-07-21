using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !SQUAX_RELEASE_MODE
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Squax.Actions
{
    [Action("Create/Log/Debug Log")]
    public class DebugLogAction : Action
    {
        public enum LogCategory
        {
            Message,
            Warning,
            Error
        }

        [SerializeField]
        private bool forceDisplayInReleaseMode = false;

        [SerializeField]
        private string message = "";

        [SerializeField]
        private LogCategory messageCategory;

        protected override void OnStart()
        {
            bool showLog = true;

#if SQUAX_RELEASE_MODE
            showLog = false;
#endif

            if (forceDisplayInReleaseMode == true)
            {
                showLog = true;
            }

            if (showLog)
            {
                switch (messageCategory)
                {
                    case LogCategory.Error:
                        Debug.LogError(message);
                        break;

                    case LogCategory.Warning:
                        Debug.LogWarning(message);
                        break;

                    case LogCategory.Message:
                        Debug.Log(message);
                        break;
                }
            }
        }

        protected override void OnUpdate()
        {
            CurrentState = State.EndNextFrame;
        }

#if !SQUAX_RELEASE_MODE
        /// <summary>
        /// Title used in tool.
        /// </summary>
        public override string Title
        {
            get
            {
                return "Debug Log";
            }
        }

        public override Vector2 Dimensions
        {
            get
            {
                return new Vector2(140, 120);
            }
        }

        override protected void OnWindowGUI(int windowID)
        {
            switch(messageCategory)
            {
                case LogCategory.Error:
                    EditorGUILayout.HelpBox(message, MessageType.Error);
                    break;

                case LogCategory.Warning:
                    EditorGUILayout.HelpBox(message, MessageType.Warning);
                    break;

                case LogCategory.Message:
                    EditorGUILayout.HelpBox(message, MessageType.Info);
                    break;
            }
        }

        override protected void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("forceDisplayInReleaseMode"), new GUIContent("Show in Release Mode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("message"), new GUIContent("Message"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("messageCategory"), new GUIContent("Category"));
        }
#endif
    }
}
