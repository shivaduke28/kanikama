using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Udon.Editor
{
    public static class UdonProgramAssetCreator
    {
        [MenuItem("Assets/Create/U# From C#", false, 5)]
        public static void CreateUdonSharp()
        {
            var selectedObject = Selection.activeObject;

            if (selectedObject == null) return;

            var scriptPath = AssetDatabase.GetAssetPath(selectedObject);
            if (!scriptPath.EndsWith(".cs"))
            {
                return;
            }

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            var assetPath = scriptPath.Replace(".cs", ".asset");
            var newProgramAsset = ScriptableObject.CreateInstance<UdonSharpProgramAsset>();
            newProgramAsset.sourceCsScript = script;

            AssetDatabase.CreateAsset(newProgramAsset, assetPath);

            AssetDatabase.Refresh();
        }
    }
}
