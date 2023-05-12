using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickActions.Editor.Data
{
    [CreateAssetMenu(menuName = "Quick Actions/Config")]
    public class QuickActionsConfig : ScriptableObject
    {
        public List<Object> quickOpenAssets = new();
        public List<string> externalLinks = new();
        public SceneListMode sceneListMode = SceneListMode.All;
        

        private const string DefaultPath = "Packages/net.harrybosch.quickactions/Editor/Data/Default Quick Actions Config.asset";
        private static QuickActionsConfig defaultAsset;
        
        internal static QuickActionsConfig GetOrCreateDefault()
        {
            if (defaultAsset) return defaultAsset;

            defaultAsset = AssetDatabase.LoadAssetAtPath<QuickActionsConfig>(DefaultPath);
            if (defaultAsset) return defaultAsset;

            defaultAsset = CreateInstance<QuickActionsConfig>();
            AssetDatabase.CreateAsset(defaultAsset, DefaultPath);
            AssetDatabase.SaveAssets();
            return defaultAsset;
        }

        public enum SceneListMode
        {
            All = default,
            BuildSettings,
            Manual,
        }
    }
}