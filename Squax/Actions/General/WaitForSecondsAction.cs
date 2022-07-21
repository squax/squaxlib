using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !SQUAX_RELEASE_MODE
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Squax.Actions
{
    [Action("Create/Time/Wait for Seconds")]
    public class WaitForSecondsAction : Action
    {
        /// <summary>
        /// Seconds to stay at node.
        /// </summary>
        [SerializeField]
        private float seconds;

        [SerializeField]
        private bool useUnscaledTime = false;

        /// <summary>
        /// The current time.
        /// </summary>
        private float currentTime;

        protected override void OnStart()
        {
            currentTime = 0;
        }

        protected override void OnUpdate()
        {
            currentTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if(currentTime >= seconds)
            {
                currentTime = 0;
                CurrentState = State.EndNextFrame;
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
                return "Wait for Seconds";
            }
        }

        public override Vector2 Dimensions
        {
            get
            {
                return new Vector2(120, 60);
            }
        }

        override protected void OnWindowGUI(int windowID)
        {
            string time = (seconds - currentTime).ToString("F");
            EditorGUILayout.LabelField("Time: " + ((CurrentState == State.Running) ? time : seconds.ToString()));
        }

        override protected void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("seconds"), new GUIContent("Seconds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useUnscaledTime"), new GUIContent("Use Unscaled Time?"));
        }
#endif
        }
}
