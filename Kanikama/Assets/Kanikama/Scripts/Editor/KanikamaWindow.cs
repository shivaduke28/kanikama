using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Kanikama.EditorOnly;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{
    class KanikamaWindow : EditorWindow
    {
        private KanikamaSceneDescriptor sceneDescriptor;
        private Scene scene;
        private bool isRunning;
        private CancellationTokenSource tokenSource;

        [MenuItem("Window/Kanikama")]
        private static void Initialize()
        {
            var window = GetWindow(typeof(KanikamaWindow));
            window.Show();
        }

        private void Awake()
        {
            titleContent.text = "Kanikama";
            scene = SceneManager.GetActiveScene();
            sceneDescriptor = FindObjectOfType<KanikamaSceneDescriptor>();
        }


        private void OnGUI()
        {
            GUILayout.Label("Bake", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Scene", scene.name);
            EditorGUI.EndDisabledGroup();
            sceneDescriptor = (KanikamaSceneDescriptor)EditorGUILayout.ObjectField("Scene Descriptor", sceneDescriptor, typeof(KanikamaSceneDescriptor), true);

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
        }

        private async void BakeAsync()
        {
            tokenSource = new CancellationTokenSource();
            var baker = new KanikamaBaker();
            isRunning = true;
            try
            {
                await baker.BakeAsync(sceneDescriptor, tokenSource.Token);

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
