using System.IO;
using UnityEditor;
using UnityEngine;

namespace QuickActions
{
    [CustomEditor(typeof(QuickActionsConfig))]
    public class QuickActionsConfigEditor : Editor
    {
        private static string lastPath;

        private void OnEnable()
        {
            lastPath = Application.dataPath;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var config = target as QuickActionsConfig;

            AddFile(config);

            AddDirectory(config);
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

        public static string RemoveFileName (string str)
        {
            var head = str.Length - 1;
            while (head > 0)
            {
                if (str[head] == '\\' || str[head] == '/') break;
                head--;
            }

            return str.Substring(0, head);
        }
    }
}
