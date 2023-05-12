using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace QuickActions.Editor.Utility
{
    public class ExternalLinks
    {
        private readonly List<string> src;
        
        private List<string> links;
        public List<string> urls, apps, assetNames, directories, filenames, invalid;

        private static readonly Regex URLRegex = new Regex(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)", RegexOptions.Compiled); 
        private static readonly Regex AppRegex = new Regex(@"[^\n*?\<>|]+(.exe|.lnk)", RegexOptions.Compiled);
        
        public ExternalLinks(IEnumerable<string> links)
        {
            src = new List<string>(links);
        }

        public void Append(string links) => src.Add(links);
        public void Append(IEnumerable<string> links) => src.AddRange(links);

        public void Finish()
        {
            links = new List<string>(src);
            
            urls = Pass(RegexCondition(URLRegex));
            apps = Pass(RegexCondition(AppRegex));
            assetNames = Pass(AssetCondition);
            directories = Pass(DirectoryCondition);
            filenames = Pass(FilenameCondition);
            invalid = Pass(All);
        }

        public static string All(string link) => link;
        private static Func<string, string> RegexCondition(Regex regex) => link => regex.IsMatch(link) ? link : null;

        private static string AssetCondition(string link) => AssetDatabase.LoadAssetAtPath(link, typeof(Object)) ? link : null;
        private static string DirectoryCondition(string link) => Directory.Exists(link) ? link : null;
        private static string FilenameCondition(string link) => File.Exists(link) ? link : null;

        private List<string> Pass(Func<string, string> predicate)
        {
            var list = new List<string>();
            var leftover = new List<string>();

            foreach (var link in links)
            {
                var entry = predicate(link);
                if (!string.IsNullOrWhiteSpace(entry)) list.Add(entry);
                else leftover.Add(link);
            }
            
            links.Clear();
            links.AddRange(leftover);
            
            return list;
        }
    }
}