﻿using System;
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
        KanikamaSceneDescriptor sceneDescriptor;
        Scene scene;
        bool isRunning;
        CancellationTokenSource tokenSource;

        TextureGenerator.Parameter texParam = new TextureGenerator.Parameter();
        bool showTextureParam;




        [MenuItem("Window/Kanikama")]
        static void Initialize()
        {
            var window = GetWindow(typeof(KanikamaWindow));
            window.Show();
        }

        void Awake()
        {
            titleContent.text = "Kanikama";
            scene = SceneManager.GetActiveScene();
            sceneDescriptor = FindObjectOfType<KanikamaSceneDescriptor>();
        }

        void OnGUI()
        {
            GUILayout.Label("Bake", EditorStyles.boldLabel);
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

            GUILayout.Label("Utility", EditorStyles.boldLabel);

            showTextureParam = EditorGUILayout.Foldout(showTextureParam, "Texture Generator");
            if (showTextureParam)
            {
                texParam.width = EditorGUILayout.IntField("width", texParam.width);
                texParam.height = EditorGUILayout.IntField("height", texParam.height);
                texParam.format = (TextureFormat)EditorGUILayout.EnumPopup("format", texParam.format);
                texParam.mipChain = EditorGUILayout.Toggle("mipChain", texParam.mipChain);
                texParam.linear = EditorGUILayout.Toggle("linear", texParam.linear);
                if (GUILayout.Button("Generate Texture"))
                {
                    var tex = TextureGenerator.GenerateTexture("Assets/tex.png", texParam);
                    Selection.activeObject = tex;
                }
            }
        }

        async void BakeAsync()
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
