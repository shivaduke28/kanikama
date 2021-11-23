using Kanikama.Baking;
using Kanikama.Baking.Bakery;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Editor
{
    public enum LightmapperType
    {
        Unity = 0,
        Bakery = 1,
    }

    class BakeWindow : EditorWindow
    {
        SceneAsset sceneAsset;
        KanikamaSceneDescriptor sceneDescriptor;
        BakeRequest bakeRequest;

        KanikamaSettings settings;

        CancellationTokenSource tokenSource;
        bool isRunning;
        Vector2 scrollPosition = new Vector2(0, 0);

        LightmapperType lightmapperType = LightmapperType.Unity;
        ILightmapper lightmapper;

        [MenuItem("Window/Kanikama/Bake")]
        static void Initialize()
        {
            var window = GetWindow(typeof(BakeWindow));
            window.Show();
        }

        void OnEnable()
        {
            titleContent.text = "Kanikama Bake";
            LoadSceneAsset();
        }

        void LoadSceneAsset()
        {
            sceneAsset = KanikamaEditorUtility.GetActiveSceneAsset();

            if (sceneAsset == null) return;

            settings = KanikamaSettings.FindSettings(sceneAsset);

            sceneDescriptor = FindObjectOfType<KanikamaSceneDescriptor>();
            if (sceneDescriptor != null)
            {
                bakeRequest = new BakeRequest(sceneDescriptor);
            }
            else
            {
                bakeRequest = null;
            }
        }

        void LoadOrCreateSettings()
        {
            settings = KanikamaSettings.FindOrCreateSettings(sceneAsset);
        }

        void OnGUI()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scroll.scrollPosition;
                {
                    DrawSceneData();
                    EditorGUILayout.Space();
                    DrawBakeRequest();
                }

                if (sceneAsset == null)
                {
                    EditorGUILayout.HelpBox("Scene is not saved as an asset.", MessageType.Warning);
                }
                else if (sceneDescriptor == null)
                {
                    EditorGUILayout.HelpBox("Kanikama Scene Descriptor is not selected.", MessageType.Warning);
                }
                else if (settings == null)
                {
                    EditorGUILayout.HelpBox("Load or Create Kanikama Settings Asset.", MessageType.Warning);
                }
                else
                {
                    if (GUILayout.Button("Bake"))
                    {
                        BakeAsync();
                    }
                }

                EditorGUILayout.Space();
                DrawAssetCreation();

                EditorGUILayout.Space();
                GUILayout.Label("Baked Assets", EditorStyles.boldLabel);

                if (GUILayout.Button("Open Assets Directory"))
                {
                    var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                    if (!string.IsNullOrEmpty(scene.path))
                    {
                        var sceneDirPath = Path.GetDirectoryName(scene.path);
                        var exportDirName = string.Format(KanikamaPath.ExportDirFormat, scene.name);
                        KanikamaEditorUtility.CreateFolderIfNecessary(sceneDirPath, exportDirName);
                        KanikamaEditorUtility.OpenDirectory(Path.Combine(sceneDirPath, exportDirName));
                    }
                }
            }
        }

        void DrawSceneData()
        {
            if (sceneDescriptor == null)
            {
                LoadSceneAsset();
            }

            GUILayout.Label("Scene", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledGroupScope(true))
            {
                sceneAsset = (SceneAsset)EditorGUILayout.ObjectField("Scene", sceneAsset, typeof(SceneAsset), false);
            }
            sceneDescriptor = (KanikamaSceneDescriptor)EditorGUILayout.ObjectField("Scene Descriptor", sceneDescriptor, typeof(KanikamaSceneDescriptor), true);
            settings = (KanikamaSettings)EditorGUILayout.ObjectField("Settings", settings, typeof(KanikamaSettings), false);

            if (GUILayout.Button("Load Active Scene"))
            {
                LoadSceneAsset();
            }

            if (settings == null)
            {
                if (GUILayout.Button("Load/Create Settings Asset"))
                {
                    LoadOrCreateSettings();
                }
            }
        }

        void DrawBakeRequest()
        {
            if (bakeRequest == null || settings == null) return;

            GUILayout.Label("Bake", EditorStyles.boldLabel);


            if (isRunning)
            {
                if (lightmapperType == LightmapperType.Unity)
                {
                    if (GUILayout.Button("Force Stop"))
                    {
                        Stop();
                    }
                    return;
                }
            }

            EditorGUI.BeginChangeCheck();
            if (IsBakeryIncluded())
            {
                lightmapperType = (LightmapperType)EditorGUILayout.EnumPopup("Lightmapper", lightmapperType);
            }
            else
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    lightmapperType = LightmapperType.Unity;
                    EditorGUILayout.EnumPopup("Lightmapper", lightmapperType);
                }
            }
            if (lightmapper == null || EditorGUI.EndChangeCheck())
            {
                lightmapper = CreateLightmapper(lightmapperType);
            }


            if (lightmapper.IsDirectionalMode())
            {
                settings.directionalMode = EditorGUILayout.Toggle("Directional Mode", settings.directionalMode);
            }
            else
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    settings.directionalMode = false;
                    settings.directionalMode = EditorGUILayout.Toggle("Directional Mode", settings.directionalMode);
                }
            }


            EditorGUILayout.Space();

            bakeRequest.isBakeAll = EditorGUILayout.Toggle("Bake All", bakeRequest.isBakeAll);

            using (new EditorGUI.DisabledGroupScope(bakeRequest.isBakeAll))
            {
                using (new EditorGUI.IndentLevelScope(EditorGUI.indentLevel++))
                {
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUILayout.LabelField("Light Source");
                    using (new EditorGUI.IndentLevelScope(indentLevel))
                    {
                        var sourceFlags = bakeRequest.lightSourceFlags;
                        var source = bakeRequest.SceneDescriptor.GetLightSources();
                        var lightCount = Mathf.Min(sourceFlags.Count, source.Count);
                        for (var i = 0; i < lightCount; i++)
                        {
                            sourceFlags[i] = EditorGUILayout.Toggle(KanikamaEditorUtility.GetName(source[i]), sourceFlags[i]);
                        }
                    }

                    EditorGUILayout.LabelField("Light Source Group");
                    using (new EditorGUI.IndentLevelScope(indentLevel))
                    {
                        var groupFlags = bakeRequest.lightSourceGroupFlags;
                        var group = bakeRequest.SceneDescriptor.GetLightSourceGroups();
                        var groupCount = Mathf.Min(groupFlags.Count, group.Count);
                        for (var i = 0; i < groupCount; i++)
                        {
                            groupFlags[i] = EditorGUILayout.Toggle(KanikamaEditorUtility.GetName(group[i]), groupFlags[i]);
                        }
                    }

                    bakeRequest.isBakeWithouKanikama = EditorGUILayout.Toggle("Non-Kanikama GI", bakeRequest.isBakeWithouKanikama);
                    bakeRequest.isGenerateAssets = EditorGUILayout.Toggle("Create Assets", bakeRequest.isGenerateAssets);
                }
            }
        }

        void DrawAssetCreation()
        {
            if (settings == null) return;
            GUILayout.Label("Asset Creation", EditorStyles.boldLabel);
            settings.createRenderTexture = EditorGUILayout.Toggle("RenderTexture", settings.createRenderTexture);
            settings.createCustomRenderTexture = EditorGUILayout.Toggle("CustomRenderTexture", settings.createCustomRenderTexture);
        }

        static bool IsBakeryIncluded()
        {
#if BAKERY_INCLUDED
            return true;
#else
            return false;
#endif
        }

        static ILightmapper CreateLightmapper(LightmapperType type)
        {
            switch (type)
            {
                case LightmapperType.Unity:
                    return new UnityLightmapper();
                case LightmapperType.Bakery:
                    return new BakeryLightmapper();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        async void BakeAsync()
        {
            bakeRequest.isDirectionalMode = settings.directionalMode;
            bakeRequest.createRenderTexture = settings.createRenderTexture;
            bakeRequest.createCustomRenderTexture = settings.createCustomRenderTexture;
            tokenSource = new CancellationTokenSource();
            var baker = new Baker(lightmapper, bakeRequest);
            isRunning = true;
            try
            {
                await baker.BakeAsync(tokenSource.Token);
                settings.UpdateAsset(baker.BakedAsset);
                EditorUtility.SetDirty(settings);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("[Kanikama] Bake canceled");
            }
            finally
            {
                isRunning = false;
            }
        }

        void Stop()
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
            }
            else
            {
                isRunning = false;
            }
        }
    }
}
