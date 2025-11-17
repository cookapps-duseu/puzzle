using CookApps.Utility;
using UnityEditor;
using UnityEngine;

namespace CookAppsEditor
{
    [CustomEditor(typeof(NonDrawingGraphic))]
    public class NonDrawingGraphicEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            EditorGUILayout.PropertyField(raycastTarget, new GUIContent("Raycast Target"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
