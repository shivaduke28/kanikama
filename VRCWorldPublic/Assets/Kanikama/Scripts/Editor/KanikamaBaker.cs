using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KanikamaGI.EditorOnly;
using UnityEditor;
using System.IO;
using VRC.Udon;
using UdonSharp;
using System.Linq;
using Kanikama.Udon;
using UdonSharpEditor;

namespace Kanikama.Editor
{
    public static class KanikamaBaker
    {
        [MenuItem("KanikamaGI/Debug")]
        public static void MyDebug()
        {
            Debug.Log(LightmapEditorSettings.maxAtlasSize);
        }

        [MenuItem("KanikamaGI/Bake")]
        public static void Bake()
        {
            var scene = SceneManager.GetActiveScene();
            var sceneData = GetSceneKanikamaData(scene);

            sceneData.SetupForBake();


            var sceneDirPath = Path.GetDirectoryName(scene.path);
            var kanikamaDirPath = Path.Combine(sceneDirPath, "Kanikama");
            AssetUtil.CreateFolderIfNecessary(sceneDirPath, "Kanikama");

            // 2d array generator
            var texture2DArrayConverters = new List<Texture2DArrayConverter>();
            var bakedAssetsDirPath = Path.Combine(sceneDirPath, scene.name.ToLower());

            // bake kanikama maps
            var kanikamaLightCount = sceneData.kanikamaLights.Count;
            for (var i = 0; i < sceneData.kanikamaLights.Count; i++)
            {
                Debug.Log($"start to bake kanikama maps {i}/{kanikamaLightCount}");
                var light = sceneData.kanikamaLights[i];
                light.enabled = true;
                Lightmapping.Bake();

                if (i == 0)
                {
                    texture2DArrayConverters = CreateTex2DArrayConverters(kanikamaDirPath, bakedAssetsDirPath);
                }

                for (var j = 0; j < texture2DArrayConverters.Count; j++)
                {
                    var bakeLightmapPath = Path.Combine(bakedAssetsDirPath, $"Lightmap-{j}_comp_light.exr");
                    var kanikamaMapPath = Path.Combine(kanikamaDirPath, $"Kanikama-{j}-{i}.exr");
                    AssetDatabase.CopyAsset(bakeLightmapPath, kanikamaMapPath);

                    var textureImporter = AssetImporter.GetAtPath(kanikamaMapPath) as TextureImporter;
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();

                    var kanikamaMap = AssetDatabase.LoadAssetAtPath<Texture2D>(kanikamaMapPath);
                    texture2DArrayConverters[j].textures.Add(kanikamaMap);
                }

                light.enabled = false;
            }


            AssetDatabase.Refresh();

            for (var j = 0; j < texture2DArrayConverters.Count; j++)
            {
                var tex2dArray = texture2DArrayConverters[j].Convert();
                var shader = Shader.Find("Kanikama/LightmapComposite");
                var mat = new Material(shader);
                var matPath = Path.Combine(kanikamaDirPath, $"KanikamaMapComposite-{j}.mat");
                AssetUtil.CreateOrReplaceAsset(ref mat, matPath);

                mat.SetTexture("_Tex2DArray", tex2dArray);
                mat.SetInt("_TexCount", sceneData.kanikamaLights.Count);

                var sceneMapPath = Path.Combine(kanikamaDirPath, $"KanikamaSceneMap-{j}.asset");
                var sceneMap = new CustomRenderTexture(tex2dArray.width, tex2dArray.height, RenderTextureFormat.ARGB32)
                {
                    material = mat,
                    updateMode = CustomRenderTextureUpdateMode.Realtime,
                    initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                    initializationColor = Color.black
                };
                AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            }


            AssetDatabase.Refresh();


            sceneData.Rollback();
            Debug.Log($"start to bake Scene GI");
            Lightmapping.Bake();
            Debug.Log($"Done");
        }

        private static SceneKanikamaData GetSceneKanikamaData(Scene scene)
        {
            var sceneData = new SceneKanikamaData();

            var allLights = Object.FindObjectsOfType<Light>();
            var kanikamaDescriptor = UdonUtil.FindUdonSharpOfType<KanikamaSceneDescriptor>();
            if (kanikamaDescriptor is null)
            {
                throw new System.Exception($"Sceneに{typeof(KanikamaSceneDescriptor).Name}オブジェクトが存在しません");

            }

            UdonSharpEditor.UdonSharpEditorUtility.CopyUdonToProxy(kanikamaDescriptor);
            sceneData.kanikamaLights.AddRange(kanikamaDescriptor.Lights);

            var kanikamaLights = sceneData.kanikamaLights;
            foreach (var light in allLights)
            {
                if (light.enabled && light.lightmapBakeType != LightmapBakeType.Realtime && !kanikamaLights.Contains(light))
                {
                    sceneData.nonKanikamaLights.Add(light);
                }
            }

            // TODO: Emissive Material

            //var allRenderers = Object.FindObjectsOfType<Renderer>();
            //foreach (var renderer in allRenderers)
            //{
            //    var flag = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            //    if (flag.HasFlag(StaticEditorFlags.LightmapStatic))
            //    {
            //        var mat = renderer.sharedMaterial;
            //        if (mat.IsKeywordEnabled("_EMISSION"))
            //        {
            //        }
            //    }

            //}



            sceneData.ambientIntensity = RenderSettings.ambientIntensity;
            return sceneData;
        }

        private static List<Texture2DArrayConverter> CreateTex2DArrayConverters(string dirPath, string bakedAssetsDirPath)
        {
            var list = new List<Texture2DArrayConverter>();
            var i = 0;
            while (AssetUtil.IsValidPath(Path.Combine(bakedAssetsDirPath, $"Lightmap-{i}_comp_light.exr")))
            {
                var converter = ScriptableObject.CreateInstance<Texture2DArrayConverter>();
                converter.fileName = $"KanikamaMapArray-{i}";
                AssetUtil.CreateOrReplaceAsset(ref converter, Path.Combine(dirPath, $"tex2DArrayGenerator-{i}.asset"));
                list.Add(converter);
                i++;
            }
            AssetDatabase.Refresh();
            return list;
        }

        public class SceneKanikamaData
        {
            public List<Light> kanikamaLights = new List<Light>();
            public List<LightData> kanikamaLightDatas = new List<LightData>();
            public List<Light> nonKanikamaLights = new List<Light>();
            public List<Renderer> nonKanikamaEmissiveRenderers = new List<Renderer>();
            public Dictionary<GameObject, Material> materialMap = new Dictionary<GameObject, Material>();
            public float ambientIntensity;

            public void SetupForBake()
            {
                RenderSettings.ambientIntensity = 0;
                foreach (var light in nonKanikamaLights)
                {
                    light.enabled = false;
                }

                foreach (var kanikama in kanikamaLights)
                {
                    var data = new LightData { color = kanikama.color, intensity = kanikama.intensity };
                    kanikamaLightDatas.Add(data);
                    kanikama.enabled = false;
                    kanikama.intensity = 1;
                    kanikama.color = Color.white;
                }
            }

            public void Rollback()
            {
                RenderSettings.ambientIntensity = ambientIntensity;
                foreach (var light in nonKanikamaLights)
                {
                    light.enabled = true;
                }

                for (var i = 0; i < kanikamaLights.Count; i++)
                {
                    var light = kanikamaLights[i];
                    var data = kanikamaLightDatas[i];
                    light.color = data.color;
                    light.intensity = data.intensity;
                }
            }
        }

        public struct LightData
        {
            public float intensity;
            public Color color;
        }

    }

}