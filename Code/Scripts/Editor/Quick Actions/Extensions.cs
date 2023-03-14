using UnityEngine;

namespace Code.Scripts.Editor.Quick_Actions
{
    public static class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent(out T component))
            {
                return component;
            }
            else return gameObject.AddComponent<T>();
        }

        public static T DeepFindCallback<T>(this Transform transform, string name, System.Func<Transform, T> callback)
        {
            var result = transform.DeepFind(name);
            if (result) return callback(result);
            else return default;
        }

        public static void DeepFindCallback(this Transform transform, string name, System.Action<Transform> callback)
        {
            var result = transform.DeepFind(name);
            if (result) callback(result);
        }

        public static Transform DeepFind(this Transform transform, string name)
        {
            var result = transform.Find(name);
            if (result) return result;

            foreach (Transform child in transform)
            {
                result = child.DeepFind(name);
                if (result) return result;
            }

            return null;
        }

        public static string Replace(this string text, System.Func<char, bool> condition, System.Func<char, string> replace)
        {
            return text.Replace((i, s) => condition(s[i]), replace);
        }
        public static string Replace(this string text, System.Func<int, string, bool> condition, System.Func<char, string> replace)
        {
            var head = 0;

            while (head < text.Length)
            {
                if (condition(head, text))
                {
                    var pre = text.Substring(0, head);
                    var post = head < text.Length - 1 ? text.Substring(head + 1, text.Length - (head + 1)) : string.Empty;
                    var mid = replace(text[head]);
                    text = pre + mid + post;
                    head += mid.Length;
                }
                else head++;
            }

            return text;
        }
        public static string Replace(this string text, char to, params char[] list)
        {
            var arr = text.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = to;
            }
            return new string(arr);
        }

        public static bool IsCapital(this char c)
        {
            return c - 65 < 26;
        }
    }
}
