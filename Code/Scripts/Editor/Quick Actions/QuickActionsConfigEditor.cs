using System.IO;
using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Editor.Quick_Actions
{
    [CustomEditor(typeof(QuickActionsConfig))]
    public class QuickActionsConfigEditor : UnityEditor.Editor
    {
        static string lastPath;

        private void OnEnable()
        {
            lastPath = Application.dataPath;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var config = target as QuickActionsConfig;

            if (GUILayout.Button("Add File..."))
            {
                var path = EditorUtility.OpenFilePanel("Add File/Application to Quick Actions", lastPath, "*");
                lastPath = Path.GetFullPath(RemoveFileName(path));
                config.externalLinks.Add(path);
            }

            if (GUILayout.Button("Add Directory..."))
            {
                var path = EditorUtility.OpenFolderPanel("Add File/Application to Quick Actions", lastPath, "");
                lastPath = Path.GetFullPath(path + "/..");
                config.externalLinks.Add(path);
            }
        }

        public static string RemoveFileName (string str)
        {
            int head = str.Length - 1;
            while (head > 0)
            {
                if (str[head] == '\\' || str[head] == '/') break;
                head--;
            }

            return str.Substring(0, head);
        }
    }
}
