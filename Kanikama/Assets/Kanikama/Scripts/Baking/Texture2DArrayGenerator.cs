using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kanikama.Baking
{
    [CreateAssetMenu(fileName = "texture2d_array_converter", menuName = "Kanikama/Texture2DArrayGenerator")]
    public class Texture2DArrayGenerator : ScriptableObject
    {
        public List<Texture2D> textures = new List<Texture2D>();
        public string fileName = "texture2d_array";

        class ShaderProperties
        {
            public static readonly int Tex0 = Shader.PropertyToID("_Tex0");
            public static readonly int Tex1 = Shader.PropertyToID("_Tex1");

            public static readonly int Tex2 = Shader.PropertyToID("_Tex2");
            public static readonly int Tex3 = Shader.PropertyToID("_Tex3");
        }

        [ContextMenu("Generate")]
        public Texture2DArray Generate()
        {
            var texArray = Generate(textures);
            var path = AssetDatabase.GetAssetPath(this);
            var dir = Path.GetDirectoryName(path);
            KanikamaEditorUtility.CreateOrReplaceAsset(ref texArray, Path.Combine(dir, $"{fileName}.asset"));
            AssetDatabase.Refresh();
            return texArray;
        }

        [ContextMenu("Generate (Linear)")]
        public Texture2DArray GenerateLinear()
        {
            var texArray = Generate(textures, true);
            var path = AssetDatabase.GetAssetPath(this);
            var dir = Path.GetDirectoryName(path);
            KanikamaEditorUtility.CreateOrReplaceAsset(ref texArray, Path.Combine(dir, $"{fileName}.asset"));
            AssetDatabase.Refresh();
            return texArray;
        }

        public static Texture2DArray PackBC6H(List<Texture2D> textures, bool isLinear = false)
        {
            var count = textures.Count;
            if (count == 0)
            {
                Debug.LogError("The size of Textures is zero.");
                return null;
            }

            // Pack 3 textures into RGB via Luminance
            // NOTE: alpha channel is not available for BC6H
            var sliceCount = Mathf.FloorToInt(count / 3f) + 1;

            var map = textures[0];
            var texArray = new Texture2DArray(map.width, map.height, sliceCount, map.format, mipChain: true, linear: false)
            {
                anisoLevel = map.anisoLevel,
                wrapMode = map.wrapMode,
                filterMode = map.filterMode,
            };

            var shader = Shader.Find("Kanikama/Pack");
            var mat = new Material(shader);

            var rtDescriptor = new RenderTextureDescriptor(map.width, map.height)
            {
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                colorFormat = RenderTextureFormat.ARGBHalf,
                useMipMap = true,
                depthBufferBits = 0,
                useDynamicScale = false,
                msaaSamples = 1,
                volumeDepth = 1
            };

            var rt = RenderTexture.GetTemporary(rtDescriptor);

            var active = RenderTexture.active;
            var param = new TextureGenerator.Parameter
            {
                Width = map.width,
                Height = map.height,
                Format = TextureFormat.RGBAHalf, // map.format should be BC6H
                Linear = false,
                MipChain = true,
            };

            for (var i = 0; i < sliceCount; i++)
            {
                var j = i * 3;
                mat.SetTexture(ShaderProperties.Tex0, GetTexture(j));
                mat.SetTexture(ShaderProperties.Tex1, GetTexture(j + 1));
                mat.SetTexture(ShaderProperties.Tex2, GetTexture(j + 2));

                var tex = TextureGenerator.GenerateTexture(param);
                RenderTexture.active = rt;
                Graphics.Blit(null, rt, mat);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, true);
                EditorUtility.CompressTexture(tex, TextureFormat.BC6H, TextureCompressionQuality.Best);
                Graphics.CopyTexture(tex, 0, texArray, i);
                DestroyImmediate(tex);
            }

            Texture GetTexture(int index)
            {
                return index < count ? textures[index] : Texture2D.blackTexture;
            }

            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(rt);
            DestroyImmediate(mat);
            return texArray;
        }

        public static Texture2DArray Generate(List<Texture2D> textures, bool isLinear = false)
        {
            var count = textures.Count;
            if (count == 0)
            {
                throw new System.Exception("テクスチャの配列が空です");
            }

            var map = textures[0];
            var texArray = new Texture2DArray(map.width, map.height, count, map.format, true, isLinear)
            {
                anisoLevel = map.anisoLevel,
                wrapMode = map.wrapMode,
                filterMode = map.filterMode,
            };
            for (var i = 0; i < count; i++)
            {
                var lightmap = textures[i];
                Graphics.CopyTexture(lightmap, 0, texArray, i);
            }

            return texArray;
        }
    }
}