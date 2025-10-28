using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace CookApps.UIExtensions.Editor
{
    [CustomEditor(typeof(CAButton))]
    public class CAButtonEditor : ButtonEditor
    {
        private SerializedProperty isBlockDragProperty;
        private SerializedProperty useDefaultClickSoundProperty;
        private SerializedProperty defaultClickSoundTypeProperty;
        private SerializedProperty forceClickableProperty;
        private SerializedProperty swappersProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            isBlockDragProperty = serializedObject.FindProperty("isBlockDrag");
            useDefaultClickSoundProperty = serializedObject.FindProperty("useDefaultClickSound");
            defaultClickSoundTypeProperty = serializedObject.FindProperty("defaultClickSoundType");
            forceClickableProperty = serializedObject.FindProperty("forceClickable");
            swappersProperty = serializedObject.FindProperty("swappers");

            TryAutoAssignSwappersForTargets();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(isBlockDragProperty);
            EditorGUILayout.PropertyField(useDefaultClickSoundProperty);
            EditorGUILayout.PropertyField(defaultClickSoundTypeProperty);
            EditorGUILayout.PropertyField(forceClickableProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            base.OnInspectorGUI();
        }

        private void TryAutoAssignSwappersForTargets()
        {
            foreach (var t in targets)
            {
                var comp = t as Component;
                if (comp == null)
                    continue;

                // Find all SimpleSwapper components under this RDButton (including inactive children)
                var found = comp.GetComponentsInChildren<SimpleSwapper>(true);

                var so = new SerializedObject(t);
                var prop = so.FindProperty("swappers");
                if (prop == null)
                    continue;

                bool needsUpdate = prop.arraySize != found.Length;
                if (!needsUpdate)
                {
                    for (int i = 0; i < found.Length; i++)
                    {
                        if (prop.GetArrayElementAtIndex(i).objectReferenceValue != found[i])
                        {
                            needsUpdate = true;
                            break;
                        }
                    }
                }

                if (needsUpdate)
                {
                    prop.arraySize = found.Length;
                    for (int i = 0; i < found.Length; i++)
                    {
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = found[i];
                    }
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(t);
                }
            }
        }
    }
}
