using System.IO;

namespace QuickActions
{
    public class Util
    {
        public static string PrettifyName(string text)
        {
            text = text.Replace(c => c.IsCapital(), c => $" {c}").Replace(' ', '_', '.', '-').Trim();
            text = text[0].ToString().ToUpper() + text.Substring(1, text.Length - 1);
            return text;
        }

        public static bool IsChildDirectoryOf(string child, string parent)
        {
            child = Path.GetFullPath(child);
            parent = Path.GetFullPath(parent);

            if (parent.Length > child.Length) return false;

            for (var i = 0; i < parent.Length; i++)
            {
                if (child[i] != parent[i]) return false;
            }

            return true;
        }
    }
}