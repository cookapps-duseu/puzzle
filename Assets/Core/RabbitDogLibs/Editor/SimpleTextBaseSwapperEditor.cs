using TMPro;
using UnityEditor;
using UnityEngine;

namespace CookApps.Editor
{
    [CustomEditor(typeof(SimpleTextBaseSwapper), true)]
    public class SimpleTextBaseSwapperEditor : UnityEditor.Editor
    {
        private SerializedProperty textProp;

        private void OnEnable()
        {
            textProp = serializedObject.FindProperty("text");
            TryAutoAssignForTargets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (textProp == null)
                textProp = serializedObject.FindProperty("text");

            if (textProp != null && textProp.objectReferenceValue == null)
            {
                var comp = target as Component;
                if (comp != null)
                {
                    var tmp = comp.GetComponent<TMP_Text>();
                    if (tmp != null)
                    {
                        textProp.objectReferenceValue = tmp;
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
                var prop = so.FindProperty("text");
                if (prop != null && prop.objectReferenceValue == null)
                {
                    var comp = t as Component;
                    if (comp != null)
                    {
                        var tmp = comp.GetComponent<TMP_Text>();
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

