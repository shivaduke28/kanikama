using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{
    public class Baker
    {
        public static class ShaderName
        {
            public const string CompositeCRT = "Kanikama/Composite/CRT";
            public const string CompositeUnlit = "Kanikama/Composite/Unlit";
            public const string Dummy = "Kanikama/Dummy";
        }

        public static class ShaderProperties
        {
            public static readonly int LighmapCount = Shader.PropertyToID("_LightmapCount");
            public static readonly int LightmapArray = Shader.PropertyToID("_LightmapArray");
            public static readonly int Lightmap = Shader.PropertyToID("_Lightmap");
        }

        readonly BakeSceneController sceneController;
        readonly BakeRequest request;
        readonly BakePath bakePath;
        public BakedAsset BakedAsset { get; }

        public Baker(BakeRequest request)
        {
            this.request = request;
            sceneController = new BakeSceneController(request.SceneDescriptor);
            bakePath = new BakePath(SceneManager.GetActiveScene());
            BakedAsset = new BakedAsset();
        }

        public async Task BakeAsync(CancellationToken token)
        {
            try
            {
                Debug.Log($"v..v Bake Started v..v");

                sceneController.Initialize();
                sceneController.TurnOff();
                sceneController.SetLightmapSettings(request.isDirectionalMode);
                await BakeLightSourcesAsync(token);
                await BakeLightSourceGroupsAsync(token);
                await BakeWithoutKanikamaAsync(token);
                CreateKanikamaAssets();

                Debug.Log($"v..v Done v..v");
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            finally
            {
                sceneController?.Rollback();
                sceneController?.Dispose();
                AssetDatabase.Refresh();
            }
        }

        async Task BakeLightSourcesAsync(CancellationToken token)
        {
            for (var i = 0; i < sceneController.LightSources.Count; i++)
            {
                if (!request.IsBakeLightSource(i)) continue;
                await BakeLightSourceAsync(i, token);
            }
        }
        async Task BakeLightSourceAsync(int index, CancellationToken token)
        {
            var source = sceneController.LightSources[index];
            var name = GetName(source);
            Debug.Log($"[Kanikama] Baking LightSource: {name}");
            source.OnBake();
            Lightmapping.Clear();
            await BakeSceneGIAsync(token);
            source.TurnOff();
            MoveBakedLightmaps(BakePath.LightSourceFormat(index));
        }

        async Task BakeLightSourceGroupsAsync(CancellationToken token)
        {
            for (var i = 0; i < sceneController.LightSourceGroups.Count; i++)
            {
                if (!request.IsBakeLightSourceGroup(i)) continue;
                await BakeLightSourceGroupAsync(i, token);
            }
        }

        async Task BakeLightSourceGroupAsync(int index, CancellationToken token)
        {
            var group = sceneController.LightSourceGroups[index];
            var name = GetName(group);

            Debug.Log($"[Kanikama] Baking LightSourceGroup: {name}");

            var sources = group.GetLightSources();

            var sourceCount = sources.Count;
            for (var i = 0; i < sourceCount; i++)
            {
                var source = sources[i];
                Debug.Log($"[Kanikama] - Baking {i}th LightSource {GetName(source)}");
                source.OnBake();
                Lightmapping.Clear();
                await BakeSceneGIAsync(token);
                source.TurnOff();
                MoveBakedLightmaps(BakePath.LightSourceGroupFormat(index, i));
            }
        }

        async Task BakeWithoutKanikamaAsync(CancellationToken token)
        {
            if (!request.IsBakeWithouKanikama()) return;
            sceneController.RollbackNonKanikama();
            sceneController.RollbackLightmapSettings();
            Debug.Log($"[Kanikama] Baking Scene GI without Kanikama...");
            Lightmapping.Clear();
            await BakeSceneGIAsync(token);
        }

        void MoveBakedLightmaps(string format)
        {
            var bakedLightmapPaths = bakePath.GetUnityLightmapPaths();
            for (var mapIndex = 0; mapIndex < bakedLightmapPaths.Count; mapIndex++)
            {
                var bakedMapPath = bakedLightmapPaths[mapIndex];
                var copiedFileName = string.Format(format, mapIndex);
                var copiedMapPath = Path.Combine(bakePath.TmpDirPath, copiedFileName);
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = AssetImporter.GetAtPath(copiedMapPath) as TextureImporter;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
            }

            if (request.isDirectionalMode)
            {
                var bakedDirectionalLightmapPaths = bakePath.GetUnityDirectionalLightmapPaths();
                for (var mapIndex = 0; mapIndex < bakedLightmapPaths.Count; mapIndex++)
                {
                    var bakedDirMapPath = bakedDirectionalLightmapPaths[mapIndex];
                    var dirFileName = $"{BakePath.DirectionalPrefix}{Path.GetFileNameWithoutExtension(string.Format(format, mapIndex))}.png";
                    var copiedDirMapPath = Path.Combine(bakePath.TmpDirPath, dirFileName);
                    AssetDatabase.CopyAsset(bakedDirMapPath, copiedDirMapPath);
                    var textureImporter = AssetImporter.GetAtPath(copiedDirMapPath) as TextureImporter;
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }
        }

        static async Task BakeSceneGIAsync(CancellationToken token)
        {
            if (!Lightmapping.BakeAsync())
                throw new TaskCanceledException("The lightmap bake job did not start successfully.");

            while (Lightmapping.isRunning)
            {
                try
                {
                    await Task.Delay(33, token);
                }
                catch (TaskCanceledException)
                {
                    Lightmapping.Cancel();
                    throw;
                }
            }
        }

        //void DeleteUnusedTextures()
        //{
        //    var lightmapCount = bakePath.GetUnityLightmapPaths().Count;
        //    var allTexturePaths = bakePath.GetAllTempTexturePaths();

        //    foreach (var path in allTexturePaths)
        //    {
        //        if (path.LightmapIndex >= lightmapCount || !sceneController.ValidateTexturePath(path))
        //        {
        //            Debug.Log($"[Kanikama] Delete Unused Texture: {path.Path}");
        //            AssetDatabase.DeleteAsset(path.Path);
        //        }
        //    }

        //    var allKanikamaPaths = bakePath.GetAllKanikamaAssetPaths();
        //    foreach (var path in allKanikamaPaths)
        //    {
        //        if (path.LightmapIndex >= lightmapCount)
        //        {
        //            Debug.Log($"[Kanikama] Delete Unused Asset: {path.Path}");
        //            AssetDatabase.DeleteAsset(path.Path);
        //        }
        //    }

        //    AssetDatabase.Refresh();
        //}

        void CreateKanikamaAssets()
        {
            if (!request.isGenerateAssets) { Debug.Log("[Kanikama] Skip creating assets"); return; }

            var lightMapCount = bakePath.GetUnityLightmapPaths().Count;
            for (var mapIndex = 0; mapIndex < lightMapCount; mapIndex++)
            {
                var textures = bakePath.GetTempLightmapPaths(mapIndex)
                    .Where(path => sceneController.ValidateTexturePath(path))
                    .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x.Path))
                    .ToList();
                if (!textures.Any()) continue;
                var texArr = Texture2DArrayGenerator.Generate(textures);
                var texArrPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.TexArrFormat, mapIndex));
                AssetUtil.CreateOrReplaceAsset(ref texArr, texArrPath);
                Debug.Log($"[Kanikama] Created {texArrPath}");
                BakedAsset.kanikamaMapArrays.Add(texArr);

                if (request.createRenderTexture)
                {
                    var (mat, rt) = CreateRTAssets(mapIndex, texArr);
                    BakedAsset.renderTextureMaterials.Add(mat);
                    BakedAsset.renderTextures.Add(rt);
                }
                if (request.createCustomRenderTexture)
                {
                    var (mat, crt) = CreateCRTAssets(mapIndex, texArr);
                    BakedAsset.customRenderTextureMaterials.Add(mat);
                    BakedAsset.customRenderTextures.Add(crt);
                }
                if (request.isDirectionalMode)
                {
                    var dirMapArray = CreateDirectionalMapArray(mapIndex);
                    BakedAsset.kanikamaDirectionalMapArrays.Add(dirMapArray);
                }
            }
            AssetDatabase.Refresh();
        }

        Texture2DArray CreateDirectionalMapArray(int mapIndex)
        {
            var dirTextures = bakePath.GetTempDirctionalMapPaths(mapIndex)
                .Where(x => sceneController.ValidateTexturePath(x))
                .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x.Path))
                .ToList();
            var dirTexArr = Texture2DArrayGenerator.Generate(dirTextures);
            var dirTexArrPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.DirTexArrFormat, mapIndex));
            AssetUtil.CreateOrReplaceAsset(ref dirTexArr, dirTexArrPath);
            Debug.Log($"[Kanikama] Created {dirTexArrPath}");
            return dirTexArr;
        }

        (Material, CustomRenderTexture) CreateCRTAssets(int mapIndex, Texture2DArray texture2DArray)
        {
            var shader = Shader.Find(ShaderName.CompositeCRT);
            var mat = new Material(shader);
            var matPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.CustomRenderTextureMaterialFormat, mapIndex));
            AssetUtil.CreateOrReplaceAsset(ref mat, matPath);
            Debug.Log($"[Kanikama] Created {matPath}");

            mat.SetInt(ShaderProperties.LighmapCount, texture2DArray.depth);
            mat.SetTexture(ShaderProperties.LightmapArray, texture2DArray);

            var sceneMapPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.CustomRenderTextureFormat, mapIndex));
            var sceneMap = new CustomRenderTexture(texture2DArray.width, texture2DArray.height, RenderTextureFormat.ARGBHalf)
            {
                useMipMap = true,
                material = mat,
                updateMode = CustomRenderTextureUpdateMode.Realtime,
                initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                initializationColor = Color.black
            };
            AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            Debug.Log($"[Kanikama] Created {sceneMapPath}");
            return (mat, sceneMap);
        }

        (Material material, RenderTexture renderTexture) CreateRTAssets(int mapIndex, Texture2DArray texture2DArray)
        {
            var shader = Shader.Find(ShaderName.CompositeUnlit);
            var mat = new Material(shader);
            var matPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.RenderTextureMaterialFormat, mapIndex));
            AssetUtil.CreateOrReplaceAsset(ref mat, matPath);
            Debug.Log($"[Kanikama] Created {matPath}");

            mat.SetInt(ShaderProperties.LighmapCount, texture2DArray.depth);
            mat.SetTexture(ShaderProperties.LightmapArray, texture2DArray);

            var sceneMapPath = Path.Combine(bakePath.ExportDirPath, string.Format(BakePath.RenderTextureFormat, mapIndex));
            var sceneMap = new RenderTexture(texture2DArray.width, texture2DArray.height, 0, RenderTextureFormat.ARGBHalf)
            {
                useMipMap = true
            };
            AssetUtil.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            Debug.Log($"[Kanikama] Created {sceneMapPath}");
            return (mat, sceneMap);
        }

        string GetName(IKanikamaLightSource source)
        {
            return source is Object ob ? ob.name : source.GetType().Name;
        }
        string GetName(IKanikamaLightSourceGroup source)
        {
            return source is Object ob ? ob.name : source.GetType().Name;
        }
    }
}