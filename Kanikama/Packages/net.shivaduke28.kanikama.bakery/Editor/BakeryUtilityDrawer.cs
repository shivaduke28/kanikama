﻿using Kanikama.Editor.GUI;
using UnityEditor;
using UnityEngine;
using GameObjectUtility = Kanikama.Editor.Utility.GameObjectUtility;

namespace Kanikama.Bakery.Editor
{
    internal class BakeryUtilityDrawer : KanikamaWindow.IGUIDrawer
    {
        [InitializeOnLoadMethod]
        static void RegisterDrawer()
        {
            KanikamaWindow.AddDrawer(KanikamaWindow.Category.Others, () => new BakeryUtilityDrawer(), 2);
        }

        bool showLightmapsStorage;

        void KanikamaWindow.IGUIDrawer.Draw()
        {
            EditorGUILayout.LabelField("Bakery Utility", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    showLightmapsStorage = EditorGUILayout.Toggle("Show !ftraceLightmaps", showLightmapsStorage);
                    if (check.changed)
                    {
                        var obj = GameObjectUtility.FindObjectOfType<ftLightmapsStorage>();
                        Debug.Log(obj == null);
                        var ft = GameObject.Find("!ftraceLightmaps");
                        if (ft != null)
                        {
                            ft.hideFlags = showLightmapsStorage ? HideFlags.None : HideFlags.HideInHierarchy;
                        }
                    }
                }
            }
        }

        void KanikamaWindow.IGUIDrawer.OnLoadActiveScene()
        {
        }
    }
}
