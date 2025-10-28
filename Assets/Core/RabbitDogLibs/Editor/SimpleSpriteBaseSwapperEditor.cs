using UnityEditor;
using UnityEngine;

namespace CookApps.Editor
{
    [CustomEditor(typeof(SimpleSpriteBaseSwapper), true)]
    public class SimpleSpriteBaseSwapperEditor : UnityEditor.Editor
    {
        private SerializedProperty spriteRendererProp;

        private void OnEnable()
        {
            spriteRendererProp = serializedObject.FindProperty("spriteRenderer");
            TryAutoAssignForTargets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (spriteRendererProp == null)
                spriteRendererProp = serializedObject.FindProperty("spriteRenderer");

            if (spriteRendererProp != null && spriteRendererProp.objectReferenceValue == null)
            {
                var comp = target as Component;
                if (comp != null)
                {
                    var sr = comp.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        spriteRendererProp.objectReferenceValue = sr;
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
                var prop = so.FindProperty("spriteRenderer");
                if (prop != null && prop.objectReferenceValue == null)
                {
                    var comp = t as Component;
                    if (comp != null)
                    {
                        var sr = comp.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            prop.objectReferenceValue = sr;
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(t);
                        }
                    }
                }
            }
        }
    }
}

