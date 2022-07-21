using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RSG;

#if !SQUAX_RELEASE_MODE
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Squax.Actions
{
    [Action("Create/Action/Start Action Container")]
    public class WaitForActionContainerAction : Action
    {
        /// <summary>
        /// Action Container to start.
        /// </summary>
        [SerializeField]
        [SelectAction]
        private ActionContainer actionContainer;

        [SerializeField]
        private bool waitForCompletition = true;

        [SerializeField]
        private bool runAsInstance = false;

        [SerializeField]
        private List<string> labels;

        [SerializeField]
        private string comment;

        private ActionContainer instance;

        protected override void OnStart()
        {
            if(runAsInstance == true)
            {
                instance = Object.Instantiate(actionContainer) as ActionContainer;
            }
            else
            {
                instance = actionContainer;
            }

            if (instance != null)
            {
                instance.StartJobs(runAsInstance);
            }
        }

        protected override void OnUpdate()
        {
            if (waitForCompletition == false)
            {
                CurrentState = State.EndNextFrame;
            }
            else if (instance != null && instance.HasJobsRunning() == false)
            {
                CurrentState = State.EndNextFrame;
            }
        }

        protected override void OnInterrupt()
        {
            CurrentState = State.Idle;

            if (instance != null)
            {
                instance.Interrupt();
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
                return "Start Action Container";
            }
        }

        public override Vector2 Dimensions
        {
            get
            {
                return new Vector2(140, 100);
            }
        }

        override protected void OnWindowGUI(int windowID)
        {
            if(string.IsNullOrEmpty(comment) == false)
            {
                EditorGUILayout.HelpBox(comment, MessageType.Info);
            }
        }

        override protected void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var labelField = "";

            foreach(var label in labels)
            {
                labelField += "l: " + label + " ";
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("labels"), new GUIContent("Label Filter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("actionContainer"), new GUIContent(labelField));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitForCompletition"), new GUIContent("Wait for Completition?"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("runAsInstance"), new GUIContent("Run as New Instance?"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("comment"), new GUIContent("Comment"));   
        }
#endif
    }
}
