using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RabbitDog.UIManagements;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace RabbitDog.Editor
{
    [InitializeOnLoad]
    internal static class UILayerAddressConstantsGenerator
    {
        [MenuItem("Tools/RabbitDog/Generate UI Layer Address Constants", priority = 2100)]
        private static void RunFromMenu()
        {
            Generate();
            Debug.Log("RabbitDog: Regenerated UI Layer Address constants.");
        }

        private const string GeneratedFolderName = "Generated";
        private const string OutputFileName = "UILayerAddressConstants.cs";

        private static readonly HashSet<AddressableAssetSettings> SubscribedSettings = new HashSet<AddressableAssetSettings>();

        static UILayerAddressConstantsGenerator()
        {
            EditorApplication.update += Initialize;
        }

        private static void Initialize()
        {
            EditorApplication.update -= Initialize;
            TrySubscribe(AddressableAssetSettingsDefaultObject.Settings);
            Generate();
        }

        private static void TrySubscribe(AddressableAssetSettings settings)
        {
            if (settings == null) return;
            if (SubscribedSettings.Contains(settings)) return;

            settings.OnModification -= HandleSettingsModification;
            settings.OnModification += HandleSettingsModification;
            SubscribedSettings.Add(settings);
        }

        private static void HandleSettingsModification(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent evt, object _)
        {
            switch (evt)
            {
                case AddressableAssetSettings.ModificationEvent.EntryAdded:
                case AddressableAssetSettings.ModificationEvent.EntryCreated:
                case AddressableAssetSettings.ModificationEvent.EntryModified:
                case AddressableAssetSettings.ModificationEvent.EntryMoved:
                case AddressableAssetSettings.ModificationEvent.EntryRemoved:
                case AddressableAssetSettings.ModificationEvent.GroupAdded:
                case AddressableAssetSettings.ModificationEvent.GroupRemoved:
                case AddressableAssetSettings.ModificationEvent.GroupRenamed:
                    TrySubscribe(settings);
                    Generate();
                    break;
            }
        }

        private static void Generate()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            TrySubscribe(settings);

            var projectFolders = ProjectFolderSettingsProvider.GetOrCreateSettings();
            if (projectFolders == null) return;

            ProjectFolderSettingsProvider.EnsureFolderHierarchy(projectFolders.RootFolder);
            ProjectFolderSettingsProvider.EnsureFolderHierarchy(projectFolders.ScriptsFolderPath);
            var outputFolder = ProjectFolderSettingsProvider.BuildChildPath(projectFolders.ScriptsFolderPath, GeneratedFolderName);
            ProjectFolderSettingsProvider.EnsureFolderHierarchy(outputFolder);
            var outputPath = ProjectFolderSettingsProvider.BuildChildPath(outputFolder, OutputFileName);

            var addresses = settings == null ? Array.Empty<PrefabAddressInfo>() : CollectPrefabAddresses(settings);
            var generationInfo = new GenerationInfo(outputPath, projectFolders.GeneratedNamespace);
            WriteConstants(addresses, generationInfo);
        }

        private static PrefabAddressInfo[] CollectPrefabAddresses(AddressableAssetSettings settings)
        {
            var collectedEntries = new List<AddressableAssetEntry>();
            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                group.GatherAllAssets(collectedEntries, true, true, false);
            }

            var results = new List<PrefabAddressInfo>();
            var seenAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var usedFieldNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var entry in collectedEntries)
            {
                if (entry == null) continue;
                if (string.IsNullOrEmpty(entry.AssetPath)) continue;
                if (!entry.AssetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrEmpty(entry.address)) continue;
                if (!seenAssetPaths.Add(entry.AssetPath)) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.AssetPath);
                if (prefab == null) continue;
                var uiLayerComponent = prefab.GetComponent<UILayer>();
                if (uiLayerComponent == null) continue;

                var prefabName = Path.GetFileNameWithoutExtension(entry.AssetPath);
                var className = uiLayerComponent.GetType().Name;
                var fieldName = BuildFieldName(className, usedFieldNames);
                results.Add(new PrefabAddressInfo(fieldName, className, prefabName, entry.address));
            }

            results.Sort((left, right) => string.CompareOrdinal(left.FieldName, right.FieldName));
            return results.ToArray();
        }

        private static string BuildFieldName(string prefabName, HashSet<string> usedNames)
        {
            if (string.IsNullOrEmpty(prefabName)) prefabName = "Prefab";

            var builder = new StringBuilder(prefabName.Length);
            foreach (var c in prefabName)
            {
                builder.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            var candidate = builder.ToString();
            if (candidate.Length == 0)
            {
                candidate = "Prefab";
            }

            if (!char.IsLetter(candidate[0]) && candidate[0] != '_')
            {
                candidate = "_" + candidate;
            }

            if (usedNames.Contains(candidate))
            {
                var index = 1;
                var baseName = candidate;
                do
                {
                    candidate = $"{baseName}_{index++}";
                } while (usedNames.Contains(candidate));
            }

            usedNames.Add(candidate);
            return candidate;
        }

        private static void WriteConstants(IReadOnlyList<PrefabAddressInfo> addresses, GenerationInfo info)
        {
            var folder = Path.GetDirectoryName(info.OutputPath);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated>");
            builder.AppendLine("// 이 파일은 UILayerAddressConstantsGenerator에 의해 자동 생성됩니다.");
            builder.AppendLine("// 수동으로 수정하지 마세요.");
            builder.AppendLine("// </auto-generated>");
            builder.AppendLine();
            builder.Append("namespace ");
            builder.Append(info.Namespace);
            builder.AppendLine();
            builder.AppendLine("{");
            builder.AppendLine("    public static class UILayerAddressConstants");
            builder.AppendLine("    {");

            if (addresses.Count == 0)
            {
                builder.AppendLine("        // UILayer가 최상위에 붙은 Addressable Prefab이 없습니다.");
            }
            else
            {
                foreach (var entry in addresses)
                {
                    builder.Append("        public const string ");
                    builder.Append(entry.FieldName);
                    builder.Append(" = \"");
                    builder.Append(entry.Address);
                    builder.Append("\"; // ");
                    builder.Append(entry.ClassName);
                    if (!string.IsNullOrEmpty(entry.PrefabName))
                    {
                        builder.Append(" (Prefab: ");
                        builder.Append(entry.PrefabName);
                        builder.Append(")");
                    }
                    builder.AppendLine();
                }
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            var newContent = builder.ToString();
            if (File.Exists(info.OutputPath))
            {
                var existing = File.ReadAllText(info.OutputPath);
                if (existing == newContent) return;
            }

            File.WriteAllText(info.OutputPath, newContent, new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string SanitizeNamespace(string value)
        {
            const string DefaultNamespace = "RabbitDog.UIManagements";

            if (string.IsNullOrWhiteSpace(value)) return DefaultNamespace;

            var segments = value.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0) return DefaultNamespace;

            var builder = new StringBuilder(value.Length);
            foreach (var segment in segments)
            {
                var sanitizedSegment = SanitizeNamespaceSegment(segment);
                if (sanitizedSegment.Length == 0) continue;
                if (builder.Length > 0) builder.Append('.');
                builder.Append(sanitizedSegment);
            }

            return builder.Length == 0 ? DefaultNamespace : builder.ToString();

            static string SanitizeNamespaceSegment(string segment)
            {
                if (string.IsNullOrEmpty(segment)) return string.Empty;

                var sb = new StringBuilder(segment.Length);
                var first = segment[0];
                sb.Append(char.IsLetter(first) || first == '_' ? first : '_');

                for (var i = 1; i < segment.Length; i++)
                {
                    var c = segment[i];
                    sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
                }

                return sb.ToString();
            }
        }

        private readonly struct PrefabAddressInfo
        {
            public PrefabAddressInfo(string fieldName, string className, string prefabName, string address)
            {
                FieldName = fieldName;
                ClassName = className;
                PrefabName = prefabName;
                Address = address;
            }

            public string FieldName { get; }
            public string ClassName { get; }
            public string PrefabName { get; }
            public string Address { get; }
        }

        private readonly struct GenerationInfo
        {
            public GenerationInfo(string outputPath, string namespaceName)
            {
                OutputPath = outputPath;
                Namespace = SanitizeNamespace(namespaceName);
            }

            public string OutputPath { get; }
            public string Namespace { get; }
        }
    }
}
