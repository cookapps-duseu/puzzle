#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CookApps.Utility.Editor
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
#endif
