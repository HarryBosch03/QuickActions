using System.Collections.Generic;
using UnityEngine;

namespace BoschingMachine.Editor.QuickActions
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Editor/Quick Actions Config")]
    public class QuickActionsConfig : ScriptableObject
    {
        public List<Object> quickOpenAssets;
        public List<string> externalLinks;
    }
}