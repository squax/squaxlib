using UnityEditor;
using UnityEngine;

namespace Squax.Actions
{
    [CustomPropertyDrawer(typeof(SelectActionAttribute))]
    public class SelectActionPropertyDrawer : PropertyDrawer
    {
        private bool pickerOpen = false;
        private float size = 300;
        private float padding = 10;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var action = property.serializedObject.targetObject as Action;

            SelectActionAttribute range = (SelectActionAttribute)attribute;

            var otherPosition = new Rect(position);

            position.width = size;

            otherPosition.width = otherPosition.width - position.width;
            otherPosition.x = position.width + padding;
            otherPosition.width = otherPosition.width - padding;

            // We are using the label.text field to add some dynamic labels support.
            if (pickerOpen == false && GUI.Button(otherPosition, "Select using Labels") == true)
            {
                pickerOpen = true;

                EditorGUIUtility.ShowObjectPicker<ActionContainer>(property.objectReferenceValue, false, label.text, 0);
            }
            else if (pickerOpen == true)
            {
                EditorGUI.LabelField(otherPosition, "Selection in progress..");
            }

            EditorGUI.PropertyField(position, property);

            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                ActionContainer actionContainer = (ActionContainer)EditorGUIUtility.GetObjectPickerObject();

                property.objectReferenceValue = actionContainer;
            }

            if (Event.current.commandName == "ObjectSelectorClosed")
            {
                pickerOpen = false;
            }
        }
    }
}