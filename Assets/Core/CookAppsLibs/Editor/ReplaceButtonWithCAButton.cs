using System.Linq;
using CookApps.UIExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CookApps.Editor
{
    public static class ConvertButtonToCAButton
    {
        // 단일 컨텍스트 메뉴 (Button 인스펙터 톱니바)
        [MenuItem("CONTEXT/Button/Convert to RDButton")]
        private static void ConvertSingle(MenuCommand command)
        {
            var btn = command.context as Button;
            if (btn == null) return;
            Convert(btn);
        }

        // 선택 영역 일괄 변환
        [MenuItem("Tools/CookApps/Convert Buttons in Selection")]
        private static void ConvertInSelection()
        {
            var targets = Selection.gameObjects?
                .SelectMany(go => go.GetComponentsInChildren<Button>(true))
                .Distinct()
                // 진짜 button들만 상속 받은 것 제외
                .Where(b => b.GetType() == typeof(Button))
                .ToArray();

            if (targets == null || targets.Length == 0)
            {
                EditorUtility.DisplayDialog("RD UI", "선택한 오브젝트 하위에 Button이 없습니다.", "OK");
                return;
            }

            foreach (var b in targets)
                Convert(b);

            EditorUtility.DisplayDialog("RD UI", $"변환 완료: {targets.Length}개", "OK");
        }

        private static void Convert(Button button)
        {
            var rdScript = FindMonoScript(typeof(CAButton));
            if (rdScript == null)
            {
                Debug.LogError("RDButton 스크립트를 찾을 수 없습니다. RDButton.cs가 프로젝트에 있고 컴파일이 끝났는지 확인하세요.");
                return;
            }

            var so = new SerializedObject(button);
            var scriptProp = so.FindProperty("m_Script");
            if (scriptProp == null)
            {
                Debug.LogError("m_Script 프로퍼티를 찾지 못했습니다. Unity 버전을 확인하세요.");
                return;
            }

            // 타입 스왑 (Undo 없이 즉시 적용)
            scriptProp.objectReferenceValue = rdScript;
            so.ApplyModifiedProperties();
        }

        private static MonoScript FindMonoScript(System.Type type)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:MonoScript {type.Name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (ms != null && ms.GetClass() == type)
                    return ms;
            }
            foreach (var ms in Resources.FindObjectsOfTypeAll<MonoScript>())
                if (ms != null && ms.GetClass() == type)
                    return ms;

            return null;
        }
    }
}