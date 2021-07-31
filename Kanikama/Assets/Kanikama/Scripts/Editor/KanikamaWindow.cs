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

        [SerializeField] BakeRequest bakeRequest;
        SerializedObject serializedObject;



        [MenuItem("Window/Kanikama")]
        static void Initialize()
        {
            var window = GetWindow(typeof(KanikamaWindow));
            window.Show();
        }

        void OnEnable()
        {
            titleContent.text = "Kanikama";
            scene = SceneManager.GetActiveScene();
            sceneDescriptor = FindObjectOfType<KanikamaSceneDescriptor>();
            if (sceneDescriptor != null)
            {
                bakeRequest = new BakeRequest(sceneDescriptor);
            }
            serializedObject = new SerializedObject(this);
        }

        void OnGUI()
        {
            serializedObject.Update();
            GUILayout.Label("Bake", EditorStyles.boldLabel);
            sceneDescriptor = (KanikamaSceneDescriptor)EditorGUILayout.ObjectField("Scene Descriptor", sceneDescriptor, typeof(KanikamaSceneDescriptor), true);

            if (sceneDescriptor != null && bakeRequest is null)
            {
                bakeRequest = new BakeRequest(sceneDescriptor);
            }


            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(bakeRequest)), true);

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

            serializedObject.ApplyModifiedProperties();
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
