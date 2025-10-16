#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;

namespace Template.Editor
{
    public static class LocalizationSync
    {
        private static readonly string StringTableCollectionPath = "Assets/_Project/Localization/Localization.asset";
        private readonly struct ExitAction : IDisposable
        {
            private readonly Action _action;
            public ExitAction(Action action)
            {
                _action = action;
            }
            public void Dispose()
            {
                _action?.Invoke();
            }
        }

        [MenuItem("Tools/Localization/로컬라이징 구글시트에서 땡기기", priority = 5000)]
        private static void SyncFromGoogleSheet()
        {
            using var _ = new ExitAction(EditorUtility.ClearProgressBar);
            EditorUtility.DisplayProgressBar("동기화중...", "잠시만 기다려 주세요...", 0);
            var localizationTableCollection =
                AssetDatabase.LoadAssetAtPath<StringTableCollection>(StringTableCollectionPath);
            if (localizationTableCollection == null)
            {
                EditorUtility.DisplayDialog("에러", "로컬라이징 테이블 컬렉션이 없습니다.", "확인");
                return;
            }
            if (localizationTableCollection.Extensions[0] is not GoogleSheetsExtension c)
            {
                EditorUtility.DisplayDialog("에러", "로컬라이징 테이블 컬렉션에 구글시트 익스텐션이 없습니다.", "확인");
                return;
            }
            var google = new GoogleSheets(c.SheetsServiceProvider)
            {
                SpreadSheetId = c.SpreadsheetId
            };
            google.PullIntoStringTableCollection(c.SheetId, c.TargetCollection as StringTableCollection, c.Columns, c.RemoveMissingPulledKeys);
            EditorUtility.DisplayDialog("완료", "로컬라이징 구글시트 동기화 완료", "확인");
        }
    }
}
#endif