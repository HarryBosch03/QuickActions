using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuickActions.Editor.Data;
using UnityEditor;
using UnityEngine;

namespace QuickActions.Editor.Interface
{
    [CustomEditor(typeof(QuickActionsConfig))]
    public class QuickActionsConfigEditor : UnityEditor.Editor
    {
        private static string lastPath;

        private void OnEnable()
        {
            lastPath = Application.dataPath;
        }

        private static void Div()
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            rect.y += rect.height * 0.5f;
            rect.height = 1.0f;
            EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.1f));
        }
        
        public override void OnInspectorGUI()
        {
            var config = target as QuickActionsConfig;

            EditorGUI.BeginChangeCheck();
            var assetList = serializedObject.FindProperty("quickOpenAssets");
            EditorGUILayout.PropertyField(assetList);
            SortListIfUserWantsTo("Sort Asset List", config, c => c.quickOpenAssets, e => e.GetType().Name);

            Div();
            
            var externalLinks = serializedObject.FindProperty("externalLinks");
            EditorGUILayout.PropertyField(externalLinks);
            SortListIfUserWantsTo("Sort Link List", config, c => c.externalLinks, e => e);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorGUI.EndChangeCheck();

            Div();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneListMode"));
            
            Div();
            
            AddFile(config);
            AddDirectory(config);
        }

        private static void SortListIfUserWantsTo<T>(string buttonName, QuickActionsConfig actions,
            Func<QuickActionsConfig, List<T>> list, Func<T, string> name)
        {
            if (!GUILayout.Button(buttonName)) return;
            if (!actions) return;

            list(actions).RemoveAll(e => e == null);
            var tmp = new List<T>(list(actions));

            list(actions).Clear();
            list(actions).AddRange(tmp.OrderBy(name));
        }

        private static void AddFile(QuickActionsConfig config)
        {
            if (!GUILayout.Button("Add File...")) return;

            var path = EditorUtility.OpenFilePanel("Add File/Application to Quick Actions", lastPath, "*");
            if (string.IsNullOrEmpty(path)) return;

            lastPath = Path.GetFullPath(RemoveFileName(path));
            config.externalLinks.Add(path);
        }

        private static void AddDirectory(QuickActionsConfig config)
        {
            if (!GUILayout.Button("Add Directory...")) return;

            var path = EditorUtility.OpenFolderPanel("Add File/Application to Quick Actions", lastPath, "");
            if (string.IsNullOrEmpty(path)) return;

            lastPath = Path.GetFullPath(path + "/..");
            config.externalLinks.Add(path);
        }

        public static string RemoveFileName(string str)
        {
            var head = str.Length - 1;
            while (head > 0)
            {
                if (str[head] == '\\' || str[head] == '/') break;
                head--;
            }

            return str[..head];
        }
    }
}