using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !SQUAX_RELEASE_MODE
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Squax.Actions
{
    [Action("Create/Input/Wait for Any Input")]
    public class WaitForAnyInputAction : Action
    {
        protected override void OnStart()
        {
        }

        protected override void OnUpdate()
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) == true || Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2") || Input.GetButtonDown("Fire3"))
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
                return "Wait for Any Input";
            }
        }

        public override Vector2 Dimensions
        {
            get
            {
                return new Vector2(120, 60);
            }
        }

        override protected void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
#endif
    }
}
