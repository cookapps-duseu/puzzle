using CookApps.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEngine.AddressableAssets;

namespace CookApps.Editor
{

[InitializeOnLoad]
public static class AddressableAssetMonitor
{
    static AddressableAssetMonitor()
    {
        // AddressableAssetSettings을 가져옵니다.
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        
        if (settings != null)
        {
            // 그룹에 대한 변경 이벤트를 등록합니다.
            settings.OnModification += OnAddressableAssetModified; 
        }
    }

    // AddressableAssetSettings 변경 시 실행되는 메서드
    private static void OnAddressableAssetModified(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent evt, object obj)
    {
        AddressableAssetEntry entry = obj as AddressableAssetEntry;
        if (entry != null)
        {
            Debug.Log($"Addressable 에셋 변경됨: {entry.address}");

            if (!entry.address.Contains("SpriteAtlas"))
                return;
            
            AtlasManagerImportProcessor.CheckAndUpdateAtlasRefs(entry.AssetPath);
        }
    }
}

public class AtlasManagerImportProcessor : AssetPostprocessor
{
    private static AtlasManagerScriptableObject cachedAtlasManager;
    private static ProjectFolderSettings projectFolderSettings;

    private static ProjectFolderSettings ProjectFolders => projectFolderSettings ??= ProjectFolderSettingsProvider.GetOrCreateSettings();

    private static string AtlasManagerAssetPath => ProjectFolderSettingsProvider.BuildChildPath(ProjectFolders.DataFolderPath, "AtlasManager.asset");

    private static AtlasManagerScriptableObject CachedAtlasManager
    {
        get
        {
            cachedAtlasManager ??= AssetDatabase.LoadAssetAtPath<AtlasManagerScriptableObject>(AtlasManagerAssetPath);
            if (cachedAtlasManager == null)
            {
                ProjectFolderSettingsProvider.EnsureFolderHierarchy(ProjectFolders.RootFolder);
                ProjectFolderSettingsProvider.EnsureFolderHierarchy(ProjectFolders.DataFolderPath);

                cachedAtlasManager = ScriptableObject.CreateInstance<AtlasManagerScriptableObject>();
                AssetDatabase.CreateAsset(cachedAtlasManager, AtlasManagerAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"RabbitDog: Created default AtlasManager asset at '{AtlasManagerAssetPath}'.");
            }

            cachedAtlasManager = AssetDatabase.LoadAssetAtPath<AtlasManagerScriptableObject>(AtlasManagerAssetPath);
            return cachedAtlasManager;
        }
    }

    public static void ClearCachedAtlasManager()
    {
        cachedAtlasManager = null;
        UpdateAtlasRefs();
    }
    
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var hasAtlasChanged = false;
        foreach (var folderPathGuid in CachedAtlasManager.folderPathGuids)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(folderPathGuid);
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.Contains(folderPath))
                {
                    hasAtlasChanged = true;
                    break;
                }
            }

            if (hasAtlasChanged)
                break;
            
            foreach (var assetPath in deletedAssets)
            {
                if (assetPath.Contains(folderPath))
                {
                    hasAtlasChanged = true;
                    break;
                }
            }

            if (hasAtlasChanged)
                break;

            foreach (var assetPath in movedAssets)
            {
                if (assetPath.Contains(folderPath))
                {
                    hasAtlasChanged = true;
                    break;
                }
            }

            if (hasAtlasChanged)
                break;
        }
        
        if (hasAtlasChanged)
        {
            UpdateAtlasRefs();
        }
    }

    public static void CheckAndUpdateAtlasRefs(string changedAssetPath)
    {
        // 변경된 에셋이 SpriteAtlas인지 확인
        foreach (var folderGuid in CachedAtlasManager.folderPathGuids)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
            if (changedAssetPath.Contains(folderPath))
            {
                UpdateAtlasRefs();
                break;
            }
        }
    }
    
    public static void UpdateAtlasRefs()
    {
        CachedAtlasManager.atlasRefs.Clear();
        foreach (var folderGuid in CachedAtlasManager.folderPathGuids)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
            string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { folderPath });
            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
                if (atlas != null)
                {
                    CachedAtlasManager.atlasRefs.Add(new AssetReferenceT<SpriteAtlas>(guid));
                }
            }
        }

        UpdateAtlasManagerScriptableObject();
    }
    
    private static void UpdateAtlasManagerScriptableObject()
    {
        // Dictionary 초기화
        CachedAtlasManager.spriteNameToAtlasDict.Clear();

        // 각 AssetReferenceT<SpriteAtlas>에서 스프라이트 이름 추출 후 딕셔너리에 저장
        foreach (var atlasRef in CachedAtlasManager.atlasRefs)
        {
            SpriteAtlas spriteAtlas = atlasRef.editorAsset;

            if (spriteAtlas != null)
            {
                Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
                spriteAtlas.GetSprites(sprites);

                foreach (var sprite in sprites)
                {
                    // 스프라이트 이름과 해당 atlasRef 저장
                    CachedAtlasManager.spriteNameToAtlasDict[sprite.name.Replace("(Clone)", "").djb2Hash()] = atlasRef.AssetGUID.djb2Hash();
                }
            }
        }

        // 변경 사항 저장
        EditorUtility.SetDirty(CachedAtlasManager);
        AssetDatabase.SaveAssets();
        Debug.Log("AtlasManagerScriptableObject가 업데이트되었습니다.");
    }
}
}
