using Kanikama.EditorOnly;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{
    class BakeWindow : EditorWindow
    {
        SceneAsset sceneAsset;
        KanikamaSceneDescriptor sceneDescriptor;
        BakeRequest bakeRequest;

        KanikamaSettings settings;

        CancellationTokenSource tokenSource;
        bool isRunning;
        Vector2 scrollPosition = new Vector2(0, 0);

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
            sceneAsset = AssetUtil.GetActiveSceneAsset();

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
                using (new EditorGUI.DisabledGroupScope(isRunning))
                {
                    DrawSceneData();
                    EditorGUILayout.Space();
                    DrawBakeRequest();
                }

                if (isRunning)
                {
                    if (GUILayout.Button("Force Stop"))
                    {
                        Stop();
                    }
                }
                else if (sceneAsset == null)
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
                    var scene = SceneManager.GetActiveScene();
                    if (!string.IsNullOrEmpty(scene.path))
                    {
                        var sceneDirPath = Path.GetDirectoryName(scene.path);
                        var exportDirName = string.Format(BakePath.ExportDirFormat, scene.name);
                        AssetUtil.CreateFolderIfNecessary(sceneDirPath, exportDirName);
                        AssetUtil.OpenDirectory(Path.Combine(sceneDirPath, exportDirName));
                    }
                }
            }
        }

        void DrawSceneData()
        {
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


            if (LightmapEditorSettings.lightmapsMode == LightmapsMode.CombinedDirectional)
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
                    var indentLevel = EditorGUI.indentLevel + 1;
                    EditorGUILayout.LabelField("Lights");
                    using (new EditorGUI.IndentLevelScope(indentLevel))
                    {
                        var lightRequests = bakeRequest.lightRequests;
                        var lights = bakeRequest.SceneDescriptor.Lights;
                        var lightCount = Mathf.Min(lightRequests.Count, lights.Count);
                        for (var i = 0; i < lightCount; i++)
                        {
                            lightRequests[i] = EditorGUILayout.Toggle(lights[i].name, lightRequests[i]);
                        }
                    }

                    EditorGUILayout.LabelField("Emissive Renderers");
                    using (new EditorGUI.IndentLevelScope(indentLevel))
                    {
                        var rendererRequests = bakeRequest.rendererRequests;
                        var renderers = bakeRequest.SceneDescriptor.EmissiveRenderers;
                        var rendererCount = Mathf.Min(rendererRequests.Count, renderers.Count);
                        for (var i = 0; i < rendererCount; i++)
                        {
                            rendererRequests[i] = EditorGUILayout.Toggle(renderers[i].name, rendererRequests[i]);
                        }
                    }
                    EditorGUILayout.LabelField("Monitors");
                    using (new EditorGUI.IndentLevelScope(indentLevel))
                    {
                        var monitorRequests = bakeRequest.monitorRequests;
                        var monitors = bakeRequest.SceneDescriptor.MonitorSetups;
                        var monitorCount = Mathf.Min(monitorRequests.Count, monitors.Count);
                        for (var i = 0; i < monitorCount; i++)
                        {
                            if (monitors[i].MainMonitor != null)
                            {
                                monitorRequests[i] = EditorGUILayout.Toggle(monitors[i].MainMonitor?.name, monitorRequests[i]);
                            }
                        }
                    }
                    using (new EditorGUI.DisabledScope(!bakeRequest.SceneDescriptor.IsAmbientEnable))
                    {
                        bakeRequest.isBakeAmbient = EditorGUILayout.Toggle("Ambient", bakeRequest.isBakeAmbient);
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

        async void BakeAsync()
        {
            bakeRequest.isDirectionalMode = settings.directionalMode;
            bakeRequest.createRenderTexture = settings.createRenderTexture;
            bakeRequest.createCustomRenderTexture = settings.createCustomRenderTexture;
            tokenSource = new CancellationTokenSource();
            var baker = new Baker(bakeRequest);
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
