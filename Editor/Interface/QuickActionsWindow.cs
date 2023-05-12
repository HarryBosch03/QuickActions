using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuickActions.Editor.Data;
using QuickActions.Editor.Utility;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace QuickActions.Editor.Interface
{
    public class QuickActionsWindow : EditorWindow
    {
        private const int ButtonHeight = 40;

        private QuickActionsConfig config;
        private UnityEditor.Editor cachedEditor;
        private bool configToggle;

        private Vector2 scrollPos;

        private readonly Dictionary<int, bool> foldoutState = new();

        private ExternalLinks links;

        [MenuItem("Tools/Quick Actions")]
        public static void Open()
        {
            var window = CreateWindow<QuickActionsWindow>("Quick Actions");
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

            UnityEditor.Editor.CreateCachedEditor(config, typeof(QuickActionsConfigEditor), ref cachedEditor);
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
                    config =
                        EditorGUILayout.ObjectField(config, typeof(QuickActionsConfig), false) as QuickActionsConfig;

                    EditorGUI.indentLevel++;
                    cachedEditor.OnInspectorGUI();

                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
            }

            DrawQuickActions();
        }

        private void DrawQuickActions()
        {
            QuickOpenAssets();
            ExternalLinks();
            Scenes();
        }

        private void Scenes()
        {
            if (!Foldout("Scenes")) return;
            
            switch (config.sceneListMode)
            {
                case QuickActionsConfig.SceneListMode.All:
                    var guids = AssetDatabase.FindAssets("t:scene");
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        DrawAsAsset(path);
                    }
                    break;
                case QuickActionsConfig.SceneListMode.BuildSettings:
                    foreach (var scene in EditorBuildSettings.scenes)
                    {
                        DrawAsAsset(scene.path);
                    }
                    break;
                default:
                case QuickActionsConfig.SceneListMode.Manual:
                    break;
            }
        }

        public bool FoldoutState(int hash)
        {
            if (!foldoutState.ContainsKey(hash)) foldoutState.Add(hash, true);
            return foldoutState[hash];
        }
        
        public bool FoldoutState(int hash, bool state)
        {
            if (!foldoutState.ContainsKey(hash)) foldoutState.Add(hash, state);
            else foldoutState[hash] = state;
            
            return state;
        }

        public bool Foldout(string label) => Foldout(label, label.GetHashCode());
        public bool Foldout(string label, int hash)
        {
            return FoldoutState(hash, EditorGUILayout.Foldout(FoldoutState(hash), label, true));
        }
        
        private void QuickOpenAssets()
        {
            if (!config) return;
            if (config.quickOpenAssets == null) return;

            var hash = 0;
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

                if (hash != asset.GetType().GetHashCode())
                {
                    hash = asset.GetType().GetHashCode();
                    
                    var header = asset.GetType().Namespace.Replace((i, s) => s[i].IsCapital(), c => $" {c}");
                    header = header.Replace("Asset", "").Trim() + "s";
                    FoldoutState(hash, EditorGUILayout.Foldout(FoldoutState(hash), header, true));
                }

                if (!foldoutState[hash]) continue;

                var icon = EditorGUIUtility.ObjectContent(asset, asset.GetType()).image;
                var folderIcon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;

                using (new EditorGUILayout.HorizontalScope())
                {
                    var rect = GUILayoutUtility.GetRect(0, ButtonHeight);

                    if (Button(new Rect(rect.x, rect.y, rect.width - rect.height * 3.0f, rect.height),
                            $"Open {asset.name}", icon))
                    {
                        if (asset is SceneAsset sceneAsset)
                        {
                            OpenSceneAsset(sceneAsset);
                            continue;
                        }

                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                        AssetDatabase.OpenAsset(asset);
                    }

                    if (Button(
                            new Rect(rect.x + (rect.width - rect.height * 3.0f), rect.y, rect.height * 2.0f,
                                rect.height), "Focus"))
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }

                    if (Button(new Rect(rect.x + (rect.width - rect.height), rect.y, rect.height, rect.height), "",
                            folderIcon))
                    {
                        OpenFileExplorerAt(AssetDatabase.GetAssetPath(asset.GetInstanceID()));
                    }
                }
            }
        }

        private static void OpenSceneAsset(SceneAsset sceneAsset)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset));
            }
        }

        private void ExternalLinks()
        {
            if (!config) return;
            if (config.externalLinks == null) return;

            links = new ExternalLinks(config.externalLinks);
            links.Finish();

            void DrawGroup(string header, List<string> list, Action<string> draw)
            {
                if (list == null || list.Count == 0) return;
                
                if (!Foldout(header, header.GetHashCode())) return;
                
                foreach (var url in list)
                {
                    draw(url);
                }
            }

            DrawGroup("URLs", links.urls, DrawAsURL);
            DrawGroup("Apps", links.apps, DrawAsApp);
            DrawGroup("Assets", links.assetNames, DrawAsAsset);
            DrawGroup("Directories", links.directories, DrawAsDirectory);
            DrawGroup("Files", links.filenames, DrawAsFile);
            DrawGroup("Invalid", links.invalid, DrawAsInvalid);
        }

        private static void DrawAsAsset(string assetName)
        {
            var asset = AssetDatabase.LoadAssetAtPath(assetName, typeof(UnityEngine.Object));

            using (new EditorGUILayout.HorizontalScope())
            {
                var icon = AssetDatabase.GetCachedIcon(assetName);
                var name = Path.GetFileName(assetName);
                if (Button($"Open {name}", icon))
                {
                    AssetDatabase.OpenAsset(asset);
                }

                icon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
                if (Button($"Goto File Location", icon))
                {
                    EditorGUIUtility.PingObject(asset);
                }
            }
        }

        private void DrawAsDirectory(string path)
        {
            var icon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
            var folderName = Directory.GetDirectories(path)[..1];
            if (Button($"Open {folderName}", icon))
            {
                OpenFileExplorerAt(path);
            }
        }

        private static void DrawAsInvalid(string path)
        {
            EditorGUILayout.HelpBox($"Failed to Parse \"{path}\"", MessageType.Error);
        }

        private void DrawAsFile(string path)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var icon = EditorGUIUtility.IconContent("d_Linked").image;
                var name = Path.GetFileName(path);
                if (Button($"Open {name}", icon))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex, this);
                    }
                }

                icon = EditorGUIUtility.IconContent("d_FolderOpened Icon").image;
                if (Button($"Goto File Location", icon))
                {
                    OpenFileExplorerAt(path);
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

        private static string TryGetApplicationPath(string subpath)
        {
            var path = Environment.GetEnvironmentVariable("appdata") + @"\Microsoft\Windows\Start Menu\Programs\";
            var searchResults = Directory.GetFiles(path, $"*{Path.GetFileNameWithoutExtension(subpath)}.*", SearchOption.AllDirectories);
            return searchResults.Length > 0 ? searchResults[0] : subpath;
        }
        
        private void DrawAsApp(string path)
        {
            var icon = EditorGUIUtility.IconContent("d_Linked").image;

            var name = Path.GetFileNameWithoutExtension(path);
            if (!Button($"Open {name}", icon)) return;

            var appPath = TryGetApplicationPath(path);
            try
            {
                System.Diagnostics.Process.Start(appPath);
            }
            catch (Exception ex)
            {
                Debug.LogError(new Exception($"Could not open application\"{appPath}\"\n", ex), this);
            }
        }

        private void DrawAsURL(string link)
        {
            var icon = EditorGUIUtility.IconContent("d_Linked").image;

            if (!Button($"Goto {link}", icon)) return;

            try
            {
                System.Diagnostics.Process.Start(link);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex, this);
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
        }

        private static bool Button(string text, Texture icon = null)
        {
            return icon
                ? GUILayout.Button(new GUIContent(text, icon), GUILayout.Height(ButtonHeight))
                : GUILayout.Button(text, GUILayout.Height(ButtonHeight));
        }

        private static bool Button(Rect rect, string text, Texture icon = null)
        {
            return icon ? GUI.Button(rect, new GUIContent(text, icon)) : GUI.Button(rect, text);
        }
    }
}