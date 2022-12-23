using Kanikama.Baking;
#if BAKERY_INCLUDED
using Kanikama.Baking.Bakery;
#endif
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
                var sourceCount = sceneDescriptor.GetLightSources().Count;
                var groupCount = sceneDescriptor.GetLightSourceGroups().Count;
                bakeRequest = new BakeRequest(sourceCount, groupCount);
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
                else if (!isRunning)
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
                sceneAsset = (SceneAsset) EditorGUILayout.ObjectField("Scene", sceneAsset, typeof(SceneAsset), false);
            }

            sceneDescriptor = (KanikamaSceneDescriptor) EditorGUILayout.ObjectField("Scene Descriptor",
                sceneDescriptor, typeof(KanikamaSceneDescriptor), true);
            settings = (KanikamaSettings) EditorGUILayout.ObjectField("Settings", settings, typeof(KanikamaSettings), false);

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
                if (settings.lightmapperType == LightmapperType.Unity)
                {
                    if (GUILayout.Button("Force Stop"))
                    {
                        Stop();
                    }
                }

                return;
            }

            EditorGUI.BeginChangeCheck();
            if (IsBakeryIncluded())
            {
                settings.lightmapperType = (LightmapperType) EditorGUILayout.EnumPopup("Lightmapper", settings.lightmapperType);
            }
            else
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    settings.lightmapperType = LightmapperType.Unity;
                    EditorGUILayout.EnumPopup("Lightmapper", settings.lightmapperType);
                }
            }

            if (lightmapper == null || EditorGUI.EndChangeCheck())
            {
                lightmapper = CreateLightmapper(settings.lightmapperType);
            }


            if (lightmapper.IsDirectionalMode())
            {
                settings.directionalMode = EditorGUILayout.Toggle("Directional Mode", settings.directionalMode);
            }
            else
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    settings.directionalMode = EditorGUILayout.Toggle("Directional Mode", false);
                }
            }

            settings.packTextures = EditorGUILayout.Toggle("Pack Textures", settings.packTextures);


            EditorGUILayout.Space();

            bakeRequest.IsBakeAll = EditorGUILayout.Toggle("Bake All", bakeRequest.IsBakeAll);

            using (new EditorGUI.DisabledGroupScope(bakeRequest.IsBakeAll))
            {
                using (new EditorGUI.IndentLevelScope(EditorGUI.indentLevel++))
                {
                    var indentLevel = EditorGUI.indentLevel;
                    EditorGUILayout.LabelField("Light Source");
                    using (new EditorGUI.IndentLevelScope(indentLevel))
                    {
                        var sourceFlags = bakeRequest.LightSourceFlags;
                        var source = sceneDescriptor.GetLightSources();
                        var lightCount = Mathf.Min(sourceFlags.Count, source.Count);
                        for (var i = 0; i < lightCount; i++)
                        {
                            sourceFlags[i] = EditorGUILayout.Toggle(KanikamaEditorUtility.GetName(source[i]), sourceFlags[i]);
                        }
                    }

                    EditorGUILayout.LabelField("Light Source Group");
                    using (new EditorGUI.IndentLevelScope(indentLevel))
                    {
                        var groupFlags = bakeRequest.LightSourceGroupFlags;
                        var group = sceneDescriptor.GetLightSourceGroups();
                        var groupCount = Mathf.Min(groupFlags.Count, group.Count);
                        for (var i = 0; i < groupCount; i++)
                        {
                            groupFlags[i] = EditorGUILayout.Toggle(KanikamaEditorUtility.GetName(group[i]), groupFlags[i]);
                        }
                    }

                    bakeRequest.IsBakeNonKanikama = EditorGUILayout.Toggle("Non-Kanikama GI", bakeRequest.IsBakeNonKanikama);
                    bakeRequest.IsGenerateAssets = EditorGUILayout.Toggle("Create Assets", bakeRequest.IsGenerateAssets);
                }
            }
        }

        void DrawAssetCreation()
        {
            if (settings == null) return;
            if (isRunning) return;
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
#if BAKERY_INCLUDED
                    return new BakeryLightmapper();
#endif
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static IKanikamaSceneManager CreateSceneManager(LightmapperType type, KanikamaSceneDescriptor sceneDescriptor)
        {
            switch (type)
            {
                case LightmapperType.Unity:
                    return new KanikamaUnitySceneManager(sceneDescriptor);
#if BAKERY_INCLUDED
                case LightmapperType.Bakery:
                    return new KanikamaBakerySceneManager(sceneDescriptor);
#endif
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        async void BakeAsync()
        {
            bakeRequest.IsDirectionalMode = settings.directionalMode;
            bakeRequest.IsCreateRenderTexture = settings.createRenderTexture;
            bakeRequest.IsCreateCustomRenderTexture = settings.createCustomRenderTexture;
            bakeRequest.IsPackTextures = settings.packTextures;
            tokenSource = new CancellationTokenSource();
            lightmapper = CreateLightmapper(settings.lightmapperType);
            var sceneManager = CreateSceneManager(settings.lightmapperType, sceneDescriptor);
            var baker = new Baker(lightmapper, sceneManager, bakeRequest);
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