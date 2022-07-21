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
    [Action("Create/Scene/Unload Scene")]
    public class UnoadSceneAction : Action
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
                asyncOp = SceneManager.UnloadSceneAsync(sceneName);
            }
            else
            {
                SceneManager.UnloadScene(sceneName);
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
                return "Unload Scene";
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadAsync"), new GUIContent("Unload Async?"));
        }
#endif
    }
}
