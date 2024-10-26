﻿using System.Linq;
using Kanikama.Utility;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    public static class ExtensionMethods
    {
        public static bool IsContributeGI(this GameObject gameObject)
        {
            var flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(gameObject);
            return flags.HasFlag(StaticEditorFlags.ContributeGI);
        }

        public static bool IsEmissiveAndContributeGI(this Renderer renderer)
        {
            return renderer.gameObject.IsContributeGI() &&
                renderer.sharedMaterials.Any(m => m.IsEmissive());
        }
    }
}