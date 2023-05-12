namespace QuickActions.Editor.Utility
{
    public static class Extensions
    {
        public static string Replace(this string text, System.Func<int, string, bool> condition, System.Func<char, string> replace)
        {
            var head = 0;

            while (head < text.Length)
            {
                if (condition(head, text))
                {
                    var pre = text[..head];
                    var post = head < text.Length - 1 ? text.Substring(head + 1, text.Length - (head + 1)) : string.Empty;
                    var mid = replace(text[head]);
                    text = pre + mid + post;
                    head += mid.Length;
                }
                else head++;
            }

            return text;
        }

        public static bool IsCapital(this char c)
        {
            return c - 65 < 26;
        }
    }
}
