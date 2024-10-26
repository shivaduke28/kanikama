﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor.GUI
{
    public sealed class KanikamaWindow : EditorWindow
    {
        public interface IGUIDrawer
        {
            void Draw();
            void OnLoadActiveScene();
        }

        sealed class DrawerFactory
        {
            public int SortingOrder { get; }
            readonly Func<IGUIDrawer> createFunc;

            public DrawerFactory(Func<IGUIDrawer> createFunc, int sortingOrder)
            {
                SortingOrder = sortingOrder;
                this.createFunc = createFunc;
            }

            public IGUIDrawer Create() => createFunc.Invoke();
        }

        public enum Category
        {
            Unity = 0,
            Bakery = 1,
            Others = 2,
        }


        static readonly Dictionary<Category, List<DrawerFactory>> Factories = new();

        static readonly GUIContent[] TabToggles =
        {
            new("Unity"),
            new("Bakery"),
            new("Others")
        };

        public static void AddDrawer(Category category, Func<IGUIDrawer> createFunc, int sortingOrder = 10)
        {
            if (!Factories.TryGetValue(category, out var factories))
            {
                factories = new List<DrawerFactory>();
                Factories[category] = factories;
            }

            factories.Add(new DrawerFactory(createFunc, sortingOrder));
        }

        [MenuItem("Window/Kanikama")]
        static void Initialize()
        {
            var window = GetWindow<KanikamaWindow>();
            window.Show();
        }

        readonly Dictionary<Category, IGUIDrawer[]> drawers = new Dictionary<Category, IGUIDrawer[]>();
        readonly Dictionary<Category, Vector2> scrollPositions = new Dictionary<Category, Vector2>();
        Category category;


        void OnEnable()
        {
            titleContent.text = "Kanikama";
            if (Factories.Count == 0) return;

            drawers.Clear();
            scrollPositions.Clear();
            category = Category.Unity;

            foreach (var kvp in Factories)
            {
                var c = kvp.Key;
                var factories = kvp.Value.OrderBy(f => f.SortingOrder).ToArray();

                drawers[c] = factories.Select(f => f.Create()).ToArray();
                scrollPositions[c] = Vector2.zero;
            }
        }

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                category = (Category) GUILayout.Toolbar((int) category, TabToggles, "LargeButton", UnityEngine.GUI.ToolbarButtonSize.Fixed);
                GUILayout.FlexibleSpace();
            }
            Draw();
        }

        void Draw()
        {
            if (!drawers.TryGetValue(category, out var guiDrawers)) return;
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPositions[category]))
            {
                scrollPositions[category] = scroll.scrollPosition;

                if (KanikamaGUI.Button("Load Active Scene"))
                {
                    foreach (var drawer in guiDrawers)
                    {
                        drawer.OnLoadActiveScene();
                    }
                }

                EditorGUILayout.Space();

                foreach (var drawer in guiDrawers)
                {
                    drawer.Draw();
                    EditorGUILayout.Space();
                }
            }
        }
    }
}
