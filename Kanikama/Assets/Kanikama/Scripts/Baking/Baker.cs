using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Baking
{
    public class Baker
    {
        public static class ShaderName
        {
            public const string CompositeCRT = "Kanikama/Composite/CRT";
            public const string CompositeUnlit = "Kanikama/Composite/Unlit";
        }

        public static class ShaderProperties
        {
            public static readonly int LighmapCount = Shader.PropertyToID("knkm_Count");
            public static readonly int LightmapArray = Shader.PropertyToID("knkm_LightmapArray");
            public static readonly int Lightmap = Shader.PropertyToID("knkm_Lightmap");
        }

        readonly IKanikamaSceneManager sceneManager;
        readonly ILightmapper lightmapper;
        readonly BakeRequest request;
        readonly KanikamaPath kanikamaPath;
        public BakedAsset BakedAsset { get; }

        public Baker(ILightmapper lightmapper, IKanikamaSceneManager sceneManager, BakeRequest request)
        {
            this.lightmapper = lightmapper;
            this.request = request;
            this.sceneManager = sceneManager;
            kanikamaPath = new KanikamaPath(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            BakedAsset = new BakedAsset();
        }

        public async Task BakeAsync(CancellationToken token)
        {
            try
            {
                Debug.Log($"v..v Bake Started v..v");

                sceneManager.Initialize();
                sceneManager.TurnOff();
                sceneManager.SetDirectionalMode(request.IsDirectionalMode);
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
                sceneManager?.Rollback();
                AssetDatabase.Refresh();
            }
        }

        async Task BakeLightSourcesAsync(CancellationToken token)
        {
            for (var i = 0; i < sceneManager.LightSources.Count; i++)
            {
                if (!request.IsBakeLightSource(i)) continue;
                await BakeLightSourceAsync(i, token);
            }
        }

        async Task BakeLightSourceAsync(int index, CancellationToken token)
        {
            var source = sceneManager.LightSources[index];
            var name = KanikamaEditorUtility.GetName(source.Value);
            Debug.Log($"[Kanikama] Baking LightSource: {name}");
            source.Value.OnBake();
            Lightmapping.Clear();
            await BakeSceneGIAsync(token);
            source.Value.TurnOff();
            MoveBakedLightmaps(KanikamaPath.LightSourceFormat(index));
        }

        async Task BakeLightSourceGroupsAsync(CancellationToken token)
        {
            for (var i = 0; i < sceneManager.LightSourceGroups.Count; i++)
            {
                if (!request.IsBakeLightSourceGroup(i)) continue;
                await BakeLightSourceGroupAsync(i, token);
            }
        }

        async Task BakeLightSourceGroupAsync(int index, CancellationToken token)
        {
            var group = sceneManager.LightSourceGroups[index];
            var name = KanikamaEditorUtility.GetName(group.Value);

            Debug.Log($"[Kanikama] Baking LightSourceGroup: {name}");

            var sourceCount = group.Value.GetLightSources().Count;
            for (var i = 0; i < sourceCount; i++)
            {
                var source = group.Value.GetLightSources()[i];
                Debug.Log($"[Kanikama] - Baking {i}th LightSource {KanikamaEditorUtility.GetName(source)}");
                source.OnBake();
                lightmapper.Clear();
                await BakeSceneGIAsync(token);
                group.Value.GetLightSources()[i].TurnOff(); // reload is necessary for Bakery
                MoveBakedLightmaps(KanikamaPath.LightSourceGroupFormat(index, i));
            }
        }

        async Task BakeWithoutKanikamaAsync(CancellationToken token)
        {
            if (!request.IsBakeWithoutKanikama()) return;
            sceneManager.RollbackNonKanikama();
            sceneManager.RollbackDirectionalMode();
            Debug.Log($"[Kanikama] Baking Scene GI without Kanikama...");
            lightmapper.Clear();
            await BakeSceneGIAsync(token);
        }

        void MoveBakedLightmaps(string format)
        {
            var bakedLightmapPaths = kanikamaPath.GetBakedLightmapPaths();
            for (var mapIndex = 0; mapIndex < bakedLightmapPaths.Count; mapIndex++)
            {
                var bakedMapPath = bakedLightmapPaths[mapIndex];
                var copiedFileName = string.Format(format, mapIndex);
                var copiedMapPath = Path.Combine(kanikamaPath.TmpDirPath, copiedFileName);
                AssetDatabase.CopyAsset(bakedMapPath, copiedMapPath);

                var textureImporter = (TextureImporter) AssetImporter.GetAtPath(copiedMapPath);
                textureImporter.isReadable = true;
                textureImporter.mipmapEnabled = true;
                textureImporter.SaveAndReimport();
            }

            if (request.IsDirectionalMode)
            {
                var bakedDirectionalLightmapPaths = kanikamaPath.GetBakedDirectionalMapPaths();
                for (var mapIndex = 0; mapIndex < bakedLightmapPaths.Count; mapIndex++)
                {
                    var bakedDirMapPath = bakedDirectionalLightmapPaths[mapIndex];
                    var dirFileName = $"{KanikamaPath.DirectionalPrefix}{Path.GetFileNameWithoutExtension(string.Format(format, mapIndex))}.png";
                    var copiedDirMapPath = Path.Combine(kanikamaPath.TmpDirPath, dirFileName);
                    AssetDatabase.CopyAsset(bakedDirMapPath, copiedDirMapPath);
                    var textureImporter = (TextureImporter) AssetImporter.GetAtPath(copiedDirMapPath);
                    textureImporter.isReadable = true;
                    textureImporter.mipmapEnabled = true;
                    textureImporter.SaveAndReimport();
                }
            }
        }

        async Task BakeSceneGIAsync(CancellationToken token)
        {
            await lightmapper.BakeAsync(token);
        }

        void CreateKanikamaAssets()
        {
            if (!request.IsGenerateAssets)
            {
                Debug.Log("[Kanikama] Skip creating assets");
                return;
            }

            var lightmapCount = kanikamaPath.GetBakedLightmapPaths().Count;
            for (var mapIndex = 0; mapIndex < lightmapCount; mapIndex++)
            {
                var textures = kanikamaPath.GetTempLightmapPaths(mapIndex)
                    .Where(ValidateTexturePath)
                    .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x.Path))
                    .ToList();
                if (mapIndex == 0)
                {
                    BakedAsset.sliceCount = textures.Count;
                }
                if (textures.Count == 0) continue;
                Texture2DArray texArr;

                if (request.IsPackTextures)
                {
                    texArr = Texture2DArrayGenerator.PackBC6H(textures);
                }
                else
                {
                    texArr = Texture2DArrayGenerator.Generate(textures);
                }
                var texArrPath = Path.Combine(kanikamaPath.ExportDirPath, string.Format(KanikamaPath.TexArrFormat, mapIndex));
                KanikamaEditorUtility.CreateOrReplaceAsset(ref texArr, texArrPath);
                Debug.Log($"[Kanikama] Created {texArrPath}");
                BakedAsset.kanikamaMapArrays.Add(texArr);

                if (request.IsCreateRenderTexture)
                {
                    var (mat, rt) = CreateRTAssets(mapIndex, texArr);
                    BakedAsset.renderTextureMaterials.Add(mat);
                    BakedAsset.renderTextures.Add(rt);
                }

                if (request.IsCreateCustomRenderTexture)
                {
                    var (mat, crt) = CreateCRTAssets(mapIndex, texArr);
                    BakedAsset.customRenderTextureMaterials.Add(mat);
                    BakedAsset.customRenderTextures.Add(crt);
                }

                if (request.IsDirectionalMode)
                {
                    var dirMapArray = CreateDirectionalMapArray(mapIndex);
                    BakedAsset.kanikamaDirectionalMapArrays.Add(dirMapArray);
                }
            }

            AssetDatabase.Refresh();
        }

        Texture2DArray CreateDirectionalMapArray(int mapIndex)
        {
            var dirTextures = kanikamaPath.GetTempDirctionalMapPaths(mapIndex)
                .Where(ValidateTexturePath)
                .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x.Path))
                .ToList();
            var dirTexArr = Texture2DArrayGenerator.Generate(dirTextures, true);
            var dirTexArrPath = Path.Combine(kanikamaPath.ExportDirPath, string.Format(KanikamaPath.DirTexArrFormat, mapIndex));
            KanikamaEditorUtility.CreateOrReplaceAsset(ref dirTexArr, dirTexArrPath);
            Debug.Log($"[Kanikama] Created {dirTexArrPath}");
            return dirTexArr;
        }

        (Material, CustomRenderTexture) CreateCRTAssets(int mapIndex, Texture2DArray texture2DArray)
        {
            var shader = Shader.Find(ShaderName.CompositeCRT);
            var mat = new Material(shader);
            var matPath = Path.Combine(kanikamaPath.ExportDirPath, string.Format(KanikamaPath.CustomRenderTextureMaterialFormat, mapIndex));
            KanikamaEditorUtility.CreateOrReplaceAsset(ref mat, matPath);
            Debug.Log($"[Kanikama] Created {matPath}");

            mat.SetInt(ShaderProperties.LighmapCount, texture2DArray.depth);
            mat.SetTexture(ShaderProperties.LightmapArray, texture2DArray);

            var sceneMapPath = Path.Combine(kanikamaPath.ExportDirPath, string.Format(KanikamaPath.CustomRenderTextureFormat, mapIndex));
            var sceneMap = new CustomRenderTexture(texture2DArray.width, texture2DArray.height, RenderTextureFormat.ARGBHalf)
            {
                useMipMap = true,
                material = mat,
                updateMode = CustomRenderTextureUpdateMode.Realtime,
                initializationMode = CustomRenderTextureUpdateMode.OnLoad,
                initializationColor = Color.black
            };
            KanikamaEditorUtility.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            Debug.Log($"[Kanikama] Created {sceneMapPath}");
            return (mat, sceneMap);
        }

        (Material material, RenderTexture renderTexture) CreateRTAssets(int mapIndex, Texture2DArray texture2DArray)
        {
            var shader = Shader.Find(ShaderName.CompositeUnlit);
            var mat = new Material(shader);
            var matPath = Path.Combine(kanikamaPath.ExportDirPath, string.Format(KanikamaPath.RenderTextureMaterialFormat, mapIndex));
            KanikamaEditorUtility.CreateOrReplaceAsset(ref mat, matPath);
            Debug.Log($"[Kanikama] Created {matPath}");

            mat.SetInt(ShaderProperties.LighmapCount, texture2DArray.depth);
            mat.SetTexture(ShaderProperties.LightmapArray, texture2DArray);

            var sceneMapPath = Path.Combine(kanikamaPath.ExportDirPath, string.Format(KanikamaPath.RenderTextureFormat, mapIndex));
            var sceneMap = new RenderTexture(texture2DArray.width, texture2DArray.height, 0, RenderTextureFormat.ARGBHalf)
            {
                useMipMap = true
            };
            KanikamaEditorUtility.CreateOrReplaceAsset(ref sceneMap, sceneMapPath);
            Debug.Log($"[Kanikama] Created {sceneMapPath}");
            return (mat, sceneMap);
        }

        public bool ValidateTexturePath(KanikamaPath.TempTexturePath pathData)
        {
            switch (pathData.Type)
            {
                case KanikamaPath.BakeTargetType.LightSource:
                    return pathData.ObjectIndex < sceneManager.LightSources.Count;
                case KanikamaPath.BakeTargetType.LightSourceGroup:
                    if (pathData.ObjectIndex >= sceneManager.LightSourceGroups.Count) return false;
                    var group = sceneManager.LightSourceGroups[pathData.ObjectIndex];
                    return pathData.SubIndex < group.Value.GetLightSources().Count;
                default:
                    return false;
            }
        }
    }
}