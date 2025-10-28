using System;
using UnityEditor;
using UnityEngine;

namespace CookApps.Editor
{
    [CreateAssetMenu(fileName = "ProjectFolderSettings", menuName = "CookApps/Project Folder Settings", order = 0)]
    public sealed class ProjectFolderSettings : ScriptableObject
    {
        [SerializeField] private string rootFolder = "Assets/_Project";
        [SerializeField] private string dataFolderName = "Data";
        [SerializeField] private string scriptsFolderName = "Scripts";
        [SerializeField] private string generatedNamespace = "CookApps.UIManagements";

        public string RootFolder => SanitizeAssetPath(rootFolder);
        public string DataFolderName => dataFolderName;
        public string DataFolderPath => CombinePaths(RootFolder, DataFolderName);
        public string ScriptsFolderName => scriptsFolderName;
        public string ScriptsFolderPath => CombinePaths(RootFolder, scriptsFolderName);
        public string GeneratedNamespace => string.IsNullOrWhiteSpace(generatedNamespace) ? "CookApps.UIManagements" : generatedNamespace.Trim();

        private static string CombinePaths(string left, string right)
        {
            left = SanitizeAssetPath(left);
            right = SanitizeAssetPath(right);

            if (string.IsNullOrEmpty(left)) return right;
            if (string.IsNullOrEmpty(right)) return left;

            if (left[^1] == '/') left = left.TrimEnd('/');
            if (right[0] == '/') right = right.TrimStart('/');

            return $"{left}/{right}";
        }

        private static string SanitizeAssetPath(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace('\\', '/');
        }
    }

    internal static class ProjectFolderSettingsProvider
    {
        private const string SettingsSearchFilter = "t:ProjectFolderSettings";
        private const string DefaultAssetPath = "Assets/Settings/ProjectFolderSettings.asset";

        private static ProjectFolderSettings cachedSettings;

        internal static ProjectFolderSettings GetOrCreateSettings()
        {
            if (cachedSettings != null) return cachedSettings;

            cachedSettings = LoadExistingSettings();
            if (cachedSettings != null) return cachedSettings;

            EnsureFolderHierarchy(System.IO.Path.GetDirectoryName(DefaultAssetPath)?.Replace('\\', '/') ?? "Assets");
            cachedSettings = ScriptableObject.CreateInstance<ProjectFolderSettings>();
            AssetDatabase.CreateAsset(cachedSettings, DefaultAssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"CookApps: Created default ProjectFolderSettings at '{DefaultAssetPath}'.");
            return cachedSettings;
        }

        internal static void EnsureFolderHierarchy(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;
            var segments = folderPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0) return;

            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = $"{current}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }
                current = next;
            }
        }

        internal static string BuildChildPath(string parent, string child)
        {
            parent = (parent ?? string.Empty).Replace('\\', '/').TrimEnd('/');
            child = (child ?? string.Empty).Replace('\\', '/').Trim('/');
            if (string.IsNullOrEmpty(parent)) return child;
            if (string.IsNullOrEmpty(child)) return parent;
            return $"{parent}/{child}";
        }

        private static ProjectFolderSettings LoadExistingSettings()
        {
            var guids = AssetDatabase.FindAssets(SettingsSearchFilter);
            if (guids == null || guids.Length == 0) return null;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var settings = AssetDatabase.LoadAssetAtPath<ProjectFolderSettings>(path);
                if (settings != null) return settings;
            }

            return null;
        }
    }
}
