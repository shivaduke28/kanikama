using UnityEngine;

namespace Kanikama.GI.Debugging
{
    [ExecuteAlways]
    [AddComponentMenu("Kanikama/GI/ColorProvider")]
    public class ColorProvider : MonoBehaviour
    {
        [SerializeField] Renderer[] renderers;
        [SerializeField] Texture2DArray[] lightmapArrays;
        [SerializeField] Texture2DArray[] directionalLightmapArrays;

        [SerializeField, ColorUsage(false, true)]
        Color[] colors;

        static readonly int LightmapArray = Shader.PropertyToID("_Udon_LightmapArray");
        static readonly int LightmapIndArray = Shader.PropertyToID("_Udon_LightmapIndArray");
        static readonly int Count = Shader.PropertyToID("_Udon_LightmapCount");
        static readonly int Colors = Shader.PropertyToID("_Udon_LightmapColors");

        Vector4[] colorsInternal;
        MaterialPropertyBlock block;


        void Start()
        {
            block = new MaterialPropertyBlock();
            foreach (var r in renderers)
            {
                var i = r.lightmapIndex;
                if (i < 0 || i > lightmapArrays.Length)
                {
                    Debug.LogWarning($"invalid lightmap index. {r.name}: {i}");
                    continue;
                }

                r.GetPropertyBlock(block);
                block.SetTexture(LightmapArray, lightmapArrays[i]);
                if (i < directionalLightmapArrays.Length)
                {
                    block.SetTexture(LightmapIndArray, directionalLightmapArrays[i]);
                }
                r.SetPropertyBlock(block);
            }
        }

        void LateUpdate()
        {
            if (colorsInternal == null || colorsInternal.Length != colors.Length)
            {
                colorsInternal = new Vector4[colors.Length];
            }
            for (var i = 0; i < colors.Length; i++)
            {
                colorsInternal[i] = colors[i];
            }

            Shader.SetGlobalVectorArray(Colors, colorsInternal);
            Shader.SetGlobalInt(Count, colorsInternal.Length);
        }
    }
}
