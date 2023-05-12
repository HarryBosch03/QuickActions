using System.IO;

namespace QuickActions.Editor.Utility
{
    public class Utility
    {
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