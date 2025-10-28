using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CookApps.Editor
{
    [CustomEditor(typeof(SimpleImageBaseSwapper), true)]
    public class SimpleImageBaseSwapperEditor : UnityEditor.Editor
    {
        private SerializedProperty imageProp;

        private void OnEnable()
        {
            imageProp = serializedObject.FindProperty("image");
            TryAutoAssignForTargets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (imageProp == null)
                imageProp = serializedObject.FindProperty("image");

            if (imageProp != null && imageProp.objectReferenceValue == null)
            {
                var comp = target as Component;
                if (comp != null)
                {
                    var img = comp.GetComponent<Image>();
                    if (img != null)
                    {
                        imageProp.objectReferenceValue = img;
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
                var prop = so.FindProperty("image");
                if (prop != null && prop.objectReferenceValue == null)
                {
                    var comp = t as Component;
                    if (comp != null)
                    {
                        var img = comp.GetComponent<Image>();
                        if (img != null)
                        {
                            prop.objectReferenceValue = img;
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(t);
                        }
                    }
                }
            }
        }
    }
}

