using CookApps.Utility;
using UnityEditor;
using UnityEngine;

namespace CookApps.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            SerializedProperty condition = property.serializedObject.FindProperty(showIf.ConditionField);

            bool shouldShow = condition is { boolValue: true };
            return shouldShow ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            SerializedProperty condition = property.serializedObject.FindProperty(showIf.ConditionField);

            if (condition is { boolValue: true })
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
