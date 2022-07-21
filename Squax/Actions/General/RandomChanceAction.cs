using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !SQUAX_RELEASE_MODE
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Squax.Actions
{
    [Action("Create/Random/Random Chance Action")]
    public class RandomChanceAction : Action
    {
        protected override void OnStart()
        {
            CurrentState = State.EndNextFrame;
        }

        protected override List<Action> OnEnd()
        {
            List<Action> chosen = new List<Action>();

            if (outNodes.Count > 0)
            {
                chosen.Add(outNodes[Random.Range(0, outNodes.Count)]);
            }

            return chosen;
        }

#if !SQUAX_RELEASE_MODE
        /// <summary>
        /// Title used in tool.
        /// </summary>
        public override string Title
        {
            get
            {
                return "Random Chance Action";
            }
        }

        public override Vector2 Dimensions
        {
            get
            {
                return new Vector2(140, 80);
            }
        }

        override protected void OnWindowGUI(int windowID)
        {
        }

        override protected void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
#endif
    }
}
