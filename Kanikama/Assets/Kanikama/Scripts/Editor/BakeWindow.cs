using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Kanikama.EditorOnly;
using UnityEngine.SceneManagement;
using System.IO;

namespace Kanikama.Editor
{
    class BakeWindow : EditorWindow
    {
        KanikamaSceneDescriptor sceneDescriptor;
        BakeRequest bakeRequest;

        CancellationTokenSource tokenSource;
        bool isRunning;
        bool showRequestDetail;
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
            sceneDescriptor = FindObjectOfType<KanikamaSceneDescriptor>();
            if (sceneDescriptor != null)
            {
                bakeRequest = new BakeRequest(sceneDescriptor);
            }
        }

        void OnGUI()
        {

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUI.BeginDisabledGroup(isRunning);

            GUILayout.Label("Bake", EditorStyles.boldLabel);
            sceneDescriptor = (KanikamaSceneDescriptor)EditorGUILayout.ObjectField("Scene Descriptor", sceneDescriptor, typeof(KanikamaSceneDescriptor), true);

            if (sceneDescriptor != null)
            {
                if (GUILayout.Button("Reload") || bakeRequest is null)
                {
                    bakeRequest = new BakeRequest(sceneDescriptor);
                }
                DrawBakeRequest();
            }


            EditorGUI.EndDisabledGroup();

            if (isRunning)
            {
                if (GUILayout.Button("Force Stop"))
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
            if (sceneDescriptor != null && !isRunning)
            {
                if (GUILayout.Button("Bake"))
                {
                    BakeAsync();
                }
            }


            GUILayout.Label("Baked Assets", EditorStyles.boldLabel);

            if (GUILayout.Button("Open Assets Directory"))
            {
                var scene = SceneManager.GetActiveScene();
                var sceneDirPath = Path.GetDirectoryName(scene.path);
                var exportDirName = string.Format(BakePath.ExportDirFormat, scene.name);
                AssetUtil.CreateFolderIfNecessary(sceneDirPath, exportDirName);
                AssetUtil.OpenDirectory(Path.Combine(sceneDirPath, exportDirName));
            }

            EditorGUILayout.EndScrollView();
        }


        void DrawBakeRequest()
        {
            GUILayout.Label("Bake Commands", EditorStyles.boldLabel);
            bakeRequest.isBakeAll = EditorGUILayout.Toggle("Bake All", bakeRequest.isBakeAll);

            showRequestDetail = EditorGUILayout.Foldout(showRequestDetail, "Bake Parts");

            if (showRequestDetail)
            {
                EditorGUI.BeginDisabledGroup(bakeRequest.isBakeAll);

                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Lights");
                EditorGUI.indentLevel++;
                var lightRequests = bakeRequest.lightRequests;
                var lights = bakeRequest.SceneDescriptor.Lights;
                var lightCount = Mathf.Min(lightRequests.Count, lights.Count);
                for (var i = 0; i < lightCount; i++)
                {
                    lightRequests[i] = EditorGUILayout.Toggle(lights[i].name, lightRequests[i]);
                }
                EditorGUI.indentLevel--;


                EditorGUILayout.LabelField("Emissive Renderers");
                EditorGUI.indentLevel++;
                var rendererRequests = bakeRequest.rendererRequests;
                var renderers = bakeRequest.SceneDescriptor.EmissiveRenderers;
                var rendererCount = Mathf.Min(rendererRequests.Count, renderers.Count);
                for (var i = 0; i < rendererCount; i++)
                {
                    rendererRequests[i] = EditorGUILayout.Toggle(renderers[i].name, rendererRequests[i]);
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Monitors");
                EditorGUI.indentLevel++;
                var monitorRequests = bakeRequest.monitorRequests;
                var monitors = bakeRequest.SceneDescriptor.MonitorSetups;
                var monitorCount = Mathf.Min(monitorRequests.Count, monitors.Count);
                for (var i = 0; i < monitorCount; i++)
                {
                    monitorRequests[i] = EditorGUILayout.Toggle(monitors[i].Renderer.name, monitorRequests[i]);
                }
                EditorGUI.indentLevel--;

                EditorGUI.BeginDisabledGroup(!bakeRequest.SceneDescriptor.IsAmbientEnable);
                bakeRequest.isBakeAmbient = EditorGUILayout.Toggle("Ambient", bakeRequest.isBakeAmbient);
                EditorGUI.EndDisabledGroup();

                bakeRequest.isGenerateAssets = EditorGUILayout.Toggle("Generate Assets", bakeRequest.isGenerateAssets);
                bakeRequest.isBakeWithouKanikama = EditorGUILayout.Toggle("Without Kanikama", bakeRequest.isBakeWithouKanikama);
                EditorGUI.indentLevel--;

                EditorGUI.EndDisabledGroup();
            }
        }

        async void BakeAsync()
        {
            tokenSource = new CancellationTokenSource();
            var baker = new Baker(bakeRequest);
            isRunning = true;
            try
            {
                await baker.BakeAsync(tokenSource.Token);

            }
            catch (TaskCanceledException)
            {
                Debug.Log("キャンセルされました");
            }
            finally
            {
                isRunning = false;
            }
        }
    }
}
