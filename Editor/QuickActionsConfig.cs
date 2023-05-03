using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickActions
{
    [CreateAssetMenu(menuName = "Quick Actions/Config")]
    public class QuickActionsConfig : ScriptableObject
    {
        public List<Object> quickOpenAssets = new();
        public List<string> externalLinks = new();

        private const string DefaultPath = "Packages/net.harrybosch.quickactions/Editor/Default Quick Actions Config.asset";
        private const string FallbackPath = "Assets/Default Quick Actions Config.asset";
        private static QuickActionsConfig defaultAsset = null;
        
        internal static QuickActionsConfig GetOrCreateDefault()
        {
            if (defaultAsset) return defaultAsset;

            defaultAsset = AssetDatabase.LoadAssetAtPath<QuickActionsConfig>(DefaultPath);
            if (defaultAsset) return defaultAsset;

            defaultAsset = CreateInstance<QuickActionsConfig>();
            AssetDatabase.CreateAsset(defaultAsset, FallbackPath);
            AssetDatabase.SaveAssets();
            return defaultAsset;
        }
    }
}