using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuickActions
{
    public class QuickActions : EditorWindow
    {
        private const int ButtonHeight = 40;

        private QuickActionsConfig config;
        private Editor cachedEditor;
        private bool configToggle;
        private readonly bool[] externalPathFoldouts = Enumerable.Repeat(true, PathTypes.count).ToArray();

        private Vector2 scrollPos;

        private readonly Dictionary<Type, bool> foldoutState = new();
        private readonly Dictionary<string, string> applicationPaths = new();

        private static readonly string[] URLTemplates = new string[]
        {
        "{0}",
        "http://{0}",
        "https://{0}",
        "http://www.{0}",
        "https://www.{0}",
        };

        private static IEnumerable<string> DirectorySearchPoints => new string[]
        {
            "",
            Environment.GetEnvironmentVariable("USERPROFILE") + "\\",
            Application.dataPath + "\\",
        };

        [MenuItem("Tools/Quick Actions")]
        public static void Open()
        {
            var window = CreateWindow<QuickActions>("Quick Actions");
            window.config = QuickActionsConfig.GetOrCreateDefault();
        }

        private void OnGUI()
        {
            using var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos);

            scrollPos = scrollView.scrollPosition;
            if (!config)
            {
                DrawNoConfig();
                return;
            }
            Editor.CreateCachedEditor(config, typeof(QuickActionsConfigEditor), ref cachedEditor);
            if (!cachedEditor)
            {
                DrawNoConfig();
                return;
            }

            configToggle = EditorGUILayout.Foldout(configToggle, "Config", true);
            if (configToggle)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUI.indentLevel++;
                    config = EditorGUILayout.ObjectField(config, typeof(QuickActionsConfig), false) as QuickActionsConfig;

                    EditorGUI.indentLevel++;
                    cachedEditor.OnInspectorGUI();

                    EditorGUI.indentLevel--;
                    if (GUILayout.Button("Purge Caches"))
                    {
                        PurgeCaches();
                    }
                    EditorGUI.indentLevel--;
                }
            }

            DrawQuickActions();
        }

        private void DrawQuickActions()
        {
            QuickOpenAssets();
            ExternalLinks();
        }

        private void QuickOpenAssets()
        {
            if (!config) return;
            if (config.quickOpenAssets == null) return;

            Type type = null;
            List<UnityEngine.Object> assetList = new();

            foreach (var asset in config.quickOpenAssets)
            {
                if (!assetList.Contains(asset)) assetList.Add(asset);
            }

            foreach (var sceneRef in EditorBuildSettings.scenes)
            {
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneRef.path);
                if (!assetList.Contains(asset)) assetList.Add(asset);
            }

            assetList = assetList.OrderBy(e => e ? e.GetType().Name : "").ToList();

            foreach (var asset in assetList)
            {
                if (!asset) continue;
                
                if (type != asset.GetType())
                {
                    type = asset.GetType();
                    if (!foldoutState.ContainsKey(type)) foldoutState.Add(type, true);

                    var header = type.Name.Replace((i, s) => s[i].IsCapital(), c => $" {c}");
                    header = header.Replace("Asset", "").Trim() + "s";
                    foldoutState[type] = EditorGUILayout.Foldout(foldoutState[type], header, true);
                }

                if (!foldoutState[type]) continue;

                var icon = EditorGUIUtility.ObjectContent(asset, asset.GetType()).image;
                var folderIcon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;

                using (new EditorGUILayout.HorizontalScope())
                {
                    var rect = GUILayoutUtility.GetRect(0, ButtonHeight);

                    if (Button(new Rect(rect.x, rect.y, rect.width - rect.height * 3.0f, rect.height), $"Open {asset.name}", icon))
                    {
                        if (asset is SceneAsset)
                        {
                            OpenSceneAsset(asset as SceneAsset);
                            continue;
                        }

                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                        AssetDatabase.OpenAsset(asset);
                    }

                    if (Button(new Rect(rect.x + (rect.width - rect.height * 3.0f), rect.y, rect.height * 2.0f, rect.height), "Focus"))
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }

                    if (Button(new Rect(rect.x + (rect.width - rect.height), rect.y, rect.height, rect.height), "", folderIcon))
                    {
                        OpenFileExplorerAt(AssetDatabase.GetAssetPath(asset.GetInstanceID()));
                    }
                }
            }
        }

        private void OpenSceneAsset(SceneAsset sceneAsset)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset));
            }
        }

        private static class PathTypes
        {
            public const int url = 0, apps = 1, pDirectories = 2, directories = 3, files = 4, invalid = 5, count = 6;

            public static readonly string[] names = new string[]
            {
            "Urls", "Applications", "Project Directories", "Directories", "Files", "Invalid"
            };
            public static readonly Action<QuickActions, string>[] DrawCalls = new Action<QuickActions, string>[]
            {
            DrawAsLink,
            DrawAsApp,
            DrawAsPDirectory,
            DrawAsDirectory,
            DrawAsFile,
            DrawAsInvalid,
            };
        }

        private void ExternalLinks()
        {
            if (!config) return;
            if (config.externalLinks == null) return;

            var pathMap = new List<string>[PathTypes.count];

            for (var i = 0; i < pathMap.Length; i++) pathMap[i] = new List<string>();

            foreach (var link in config.externalLinks)
            {
                if (string.IsNullOrEmpty(link)) continue;
                pathMap[GetPathTypeIndex(link, out var res)].Add(res);
            }

            for (var i = 0; i < PathTypes.count; i++)
            {
                if (pathMap[i].Count == 0) continue;

                if (i == PathTypes.invalid)
                {
                    EditorGUILayout.HelpBox($"Some Links are Invalid", MessageType.Error);
                }

                if (!(externalPathFoldouts[i] = EditorGUILayout.Foldout(externalPathFoldouts[i], PathTypes.names[i], true))) continue;

                foreach (var path in pathMap[i])
                {
                    PathTypes.DrawCalls[i](this, path);
                }
            }
        }

        private bool IsUrl(string link)
        {
            bool IsUrl(string link)
            {
                if (Uri.TryCreate(link, UriKind.Absolute, out var result))
                {
                    return result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps;
                }
                return false;
            }

            foreach (var template in URLTemplates)
            {
                if (IsUrl(string.Format(template, link))) return true;
            }

            return false;
        }

        private int GetPathTypeIndex(string path, out string res)
        {
            static int ParseDir(string p)
            {
                if (Util.IsChildDirectoryOf(p, Application.dataPath)) return PathTypes.pDirectories;
                return PathTypes.directories;
            }

            foreach (var root in DirectorySearchPoints)
            {
                var subPath = root + path;
                if (TryGetAppPath(subPath, out res))
                {
                    if (!applicationPaths.ContainsKey(path)) applicationPaths.Add(path, res);
                    return PathTypes.apps;
                }

                res = subPath;
                if (File.Exists(subPath)) return PathTypes.files;
                if (Directory.Exists(subPath)) return ParseDir(subPath);
            }

            res = path;
            if (IsUrl(path)) return PathTypes.url;

            return PathTypes.invalid;
        }

        private static void DrawAsPDirectory(QuickActions quickActions, string path)
        {
            var icon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
            var folderName = path.Split('\\', '/').Last();
            if (quickActions.Button($"Open {folderName}", icon))
            {
                quickActions.OpenFileExplorerAt(path);
            }
        }

        private static void DrawAsDirectory(QuickActions quickActions, string path)
        {
            var icon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
            var folderName = path.Split('\\', '/').Last();
            if (quickActions.Button($"Open {folderName}", icon))
            {
                quickActions.OpenFileExplorerAt(path);
            }
        }

        private static void DrawAsInvalid(QuickActions quickActions, string path)
        {
            EditorGUI.indentLevel++;
            GUILayout.Label($"Failed to Parse \"{path}\"");
            EditorGUI.indentLevel--;
        }

        private static void DrawAsFile(QuickActions quickActions, string path)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var icon = EditorGUIUtility.IconContent("d_Linked").image;
                var name = Path.GetFileName(path);
                if (quickActions.Button($"Open {name}", icon))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex, quickActions);
                    }
                }

                icon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
                if (quickActions.Button($"Goto File Location", icon))
                {
                    quickActions.OpenFileExplorerAt(path);
                }
            }
        }

        private void OpenFileExplorerAt(string path)
        {
            path = Path.GetFullPath(path);
            try
            {
                if (File.Exists(path))
                {
                    var augment = $"/select, \"{path}\"";
                    System.Diagnostics.Process.Start("explorer.exe", augment);
                }
                else if (Directory.Exists(path))
                {
                    var augment = $"\"{path}\"";
                    System.Diagnostics.Process.Start("explorer.exe", augment);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex, this);
            }
        }

        private string[] ShortcutPaths => new string[]
        {
        Environment.ExpandEnvironmentVariables(@"%appdata%\Microsoft\Windows\Start Menu\Programs"),
        @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
        };

        private bool TryGetAppPath(string path, out string res)
        {
            if (applicationPaths.ContainsKey(path))
            {
                res = applicationPaths[path];
                return true;
            }

            var extension = Path.GetExtension(path);

            if (extension == ".exe" || extension == ".lnk")
            {
                if (File.Exists(path))
                {
                    res = path;
                    return true;
                }
            }

            if (path.Contains('\\'))
            {
                res = path;
                return false;
            }

            if (TrySearchForShortcuts(path, out res))
            {
                return true;
            }
            return false;
        }

        private bool TrySearchForShortcuts(string name, out string res)
        {
            var extension = Path.GetExtension(name);
            if (extension == "") name += ".lnk";
            else if (extension != ".lnk")
            {
                res = name;
                return false;
            }

            var files = new List<string>();
            var paths = ShortcutPaths;

            var fileName = Path.GetFileName(name);
            var append = name.Substring(0, name.Length - fileName.Length);

            foreach (var path in paths)
            {
                if (Directory.Exists(path + append))
                {
                    files.AddRange(Directory.GetFiles(path + append, fileName, SearchOption.AllDirectories));
                }
            }

            if (files.Count == 0)
            {
                res = name;
                return false;
            }

            res = files[0];
            return true;
        }

        private static void DrawAsApp(QuickActions quickActions, string path)
        {
            var icon = EditorGUIUtility.IconContent("d_Linked").image;

            var name = Path.GetFileNameWithoutExtension(path);
            if (quickActions.Button($"Open {name}", icon))
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex, quickActions);
                }
            }
        }

        private static void DrawAsLink(QuickActions quickActions, string link)
        {
            var icon = EditorGUIUtility.IconContent("d_Linked").image;

            if (quickActions.Button($"Goto {link}", icon))
            {
                try
                {
                    System.Diagnostics.Process.Start(link);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex, quickActions);
                }
            }
        }

        private void DrawNoConfig()
        {
            config = EditorGUILayout.ObjectField(config, typeof(QuickActionsConfig), false) as QuickActionsConfig;
            EditorGUILayout.HelpBox("Missing Config", MessageType.Warning);
            if (Button("Load Default"))
            {
                config = QuickActionsConfig.GetOrCreateDefault();
            }
            return;
        }

        private bool Button(string text, Texture icon = null)
        {
            if (icon) return GUILayout.Button(new GUIContent(text, icon), GUILayout.Height(ButtonHeight));
            else return GUILayout.Button(text, GUILayout.Height(ButtonHeight));
        }
        private bool Button(Rect rect, string text, Texture icon = null)
        {
            if (icon) return GUI.Button(rect, new GUIContent(text, icon));
            else return GUI.Button(rect, text);
        }

        private void PurgeCaches()
        {
            foldoutState.Clear();
            applicationPaths.Clear();
        }
    }
}