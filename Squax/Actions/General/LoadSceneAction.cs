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
    [Action("Create/Scene/Load Scene")]
    public class LoadSceneAction : Action
    {
        [SerializeField]
        private string sceneName = "";

        [SerializeField]
        private LoadSceneMode loadSceneMode;

        [SerializeField]
        private bool loadAsync = true;

        private AsyncOperation asyncOp;

        protected override void OnStart()
        {
            if (loadAsync == true)
            {
                asyncOp = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            }
            else
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
            }
        }

        protected override void OnUpdate()
        {
            if (loadAsync == true && asyncOp.isDone == true)
            {
                CurrentState = State.EndNextFrame;
            }
            else if (loadAsync == false)
            {
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
                return "Load Scene";
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
        }

        override protected void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneName"), new GUIContent("Scene Name"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadSceneMode"), new GUIContent("Load Scene Mode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadAsync"), new GUIContent("Load Async?"));
        }
#endif
    }
}
