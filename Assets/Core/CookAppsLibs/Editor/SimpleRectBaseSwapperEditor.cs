using TMPro;
using UnityEditor;
using UnityEngine;

namespace CookApps.Editor
{
    [CustomEditor(typeof(CookApps.SimpleRectBaseSwapper), true)]
    public class SimpleRectBaseSwapperEditor : UnityEditor.Editor
    {
        private SerializedProperty rectTrProp;

        private void OnEnable()
        {
            rectTrProp = serializedObject.FindProperty("rectTr");
            TryAutoAssignForTargets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (rectTrProp == null)
                rectTrProp = serializedObject.FindProperty("rectTr");

            if (rectTrProp != null && rectTrProp.objectReferenceValue == null)
            {
                var comp = target as Component;
                if (comp != null)
                {
                    var tmp = comp.GetComponent<RectTransform>();
                    if (tmp != null)
                    {
                        rectTrProp.objectReferenceValue = tmp;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                }
            }

            DrawDefaultInspector();
        }

        private void TryAutoAssignForTargets()
        {
            foreach (var t in targets)
            {
                var so = new SerializedObject(t);
                var prop = so.FindProperty("rectTr");
                if (prop != null && prop.objectReferenceValue == null)
                {
                    var comp = t as Component;
                    if (comp != null)
                    {
                        var tmp = comp.GetComponent<RectTransform>();
                        if (tmp != null)
                        {
                            prop.objectReferenceValue = tmp;
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(t);
                        }
                    }
                }
            }
        }
    }
}

