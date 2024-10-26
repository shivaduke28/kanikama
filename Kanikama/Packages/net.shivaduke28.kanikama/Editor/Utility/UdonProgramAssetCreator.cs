using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    internal static class UdonProgramAssetCreator
    {
        [MenuItem("Assets/Create/U# From C#", false, 5)]
        public static void CreateUdonSharp()
        {
            var selectedObjects = Selection.objects;
            foreach (var selected in selectedObjects)
            {
                var scriptPath = AssetDatabase.GetAssetPath(selected);
                if (!scriptPath.EndsWith(".cs")) continue;

                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                var assetPath = scriptPath.Replace(".cs", ".asset");
                if (AssetDatabase.LoadAssetAtPath<UdonSharpProgramAsset>(assetPath) != null) continue;

                var newProgramAsset = ScriptableObject.CreateInstance<UdonSharpProgramAsset>();
                newProgramAsset.sourceCsScript = script;
                AssetDatabase.CreateAsset(newProgramAsset, assetPath);
            }
            AssetDatabase.Refresh();
        }
    }
}
