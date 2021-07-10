using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using KanikamaGI.EditorOnly;
using UnityEditor;
using System.IO;

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
            var tex2dArrayGenerator = new Texture2DArrayConverter { fileName = "KanikamaMapArray-0" };
            var bakedAssetsDirPath = Path.Combine(sceneDirPath, scene.name.ToLower());

            // bake kanikama maps
            var kanikamaLightCount = sceneData.kanikamaLights.Count;
            for (var i = 0; i < sceneData.kanikamaLights.Count; i++)
            {
                Debug.Log($"start to bake kanikama maps {i}/{kanikamaLightCount}");
                var light = sceneData.kanikamaLights[i];
                light.enabled = true;
                Lightmapping.Bake();

                var bakeLightmapPath = Path.Combine(bakedAssetsDirPath, "Lightmap-0_comp_light.exr");
                var kanikamaMapPath = Path.Combine(kanikamaDirPath, $"Kanikama{i}-0.exr");
                AssetDatabase.CopyAsset(bakeLightmapPath, kanikamaMapPath);

                var textureImporter = AssetImporter.GetAtPath(kanikamaMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();

                var kanikamaMap = AssetDatabase.LoadAssetAtPath<Texture2D>(kanikamaMapPath);
                tex2dArrayGenerator.textures.Add(kanikamaMap);

                light.enabled = false;
            }


            AssetUtil.CreateOrReplaceAsset(ref tex2dArrayGenerator, Path.Combine(kanikamaDirPath, "tex2DArrayGenerator.asset"));
            AssetDatabase.Refresh();

            var tex2dArray = tex2dArrayGenerator.Convert();
            var shader = Shader.Find("Kanikama/LightmapComposite");
            var mat = new Material(shader);
            var matPath = Path.Combine(kanikamaDirPath, "KanikamaMapComposite-0.mat");
            AssetUtil.CreateOrReplaceAsset(ref mat, matPath);

            mat.SetTexture("_Tex2DArray", tex2dArray);
            mat.SetInt("_TexCount", sceneData.kanikamaLights.Count);

            var sceneMapPath = Path.Combine(kanikamaDirPath, "KanikamaSceneMap.asset");
            var sceneMap = new CustomRenderTexture(tex2dArray.width, tex2dArray.height, RenderTextureFormat.ARGB32)
            {
                material = mat,
                updateMode = CustomRenderTextureUpdateMode.Realtime,
                initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                initializationColor = Color.black
            };
            AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
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
            var kanikamaReference = Object.FindObjectOfType<KanikamaLightReference>();


            if (!kanikamaReference)
            {
                throw new System.Exception("SceneにKanikamaLightReferenceオブジェクトが存在しません");
            }

            sceneData.kanikamaLights.AddRange(kanikamaReference.lights);

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

                for(var i = 0; i < kanikamaLights.Count; i++)
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