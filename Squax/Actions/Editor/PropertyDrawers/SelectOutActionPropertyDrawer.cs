using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Squax.Actions
{
    [CustomPropertyDrawer(typeof(SelectOutActionAttribute))]
    public class SelectOutActionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
#if !SQUAX_RELEASE_MODE
            var action = property.serializedObject.targetObject as Action;

            var nameList = new List<string>();

            nameList.Add("<None Selected>");

            int selectedIndex = 0;
            foreach (var act in action.Out)
            {
                if(act == property.objectReferenceValue)
                {
                    selectedIndex = nameList.Count;
                }

                nameList.Add((nameList.Count) + ") " + act.Title);
            }

            position.width = position.width / 3;

            EditorGUI.LabelField(position, label.text);

            position.x = position.x + position.width;

            selectedIndex = EditorGUI.Popup(position, selectedIndex, nameList.ToArray());

            if (selectedIndex > 0)
            {
                property.objectReferenceValue = action.Out[selectedIndex-1];
            }
            else if (selectedIndex == 0)
            {
                property.objectReferenceValue = null;
            }

            position.x = position.x + position.width;


            if (Action.CopyAction != null && action.Out.Contains(Action.CopyAction) && GUI.Button(position, "Paste Node"))
            {
                property.objectReferenceValue = Action.CopyAction;
            }
#endif
        }
    }
}