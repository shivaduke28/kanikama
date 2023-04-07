using System.Collections.Generic;
using UnityEngine;

namespace Kanikama.GI.Runtime.Impl
{
    [AddComponentMenu("Kanikama/GI/Runtime/KanikamaGIUpdater")]
    public class KanikamaGIUpdater : MonoBehaviour
    {
        [SerializeField] KanikamaSceneDescriptor kanikamaSceneDescriptor;
        [SerializeField] Renderer[] renderers;
        [SerializeField] Texture2DArray[] lightmapArrays;
        [SerializeField] Texture2DArray[] directionalLightmapArrays;

        static readonly int LightmapArray = Shader.PropertyToID("_Udon_LightmapArray");
        static readonly int LightmapIndArray = Shader.PropertyToID("_Udon_LightmapIndArray");
        static readonly int Count = Shader.PropertyToID("_Udon_LightmapCount");
        static readonly int Colors = Shader.PropertyToID("_Udon_LightmapColors");

        Vector4[] colorsInternal;
        MaterialPropertyBlock block;
        List<ILightSource> lightSources;


        void Start()
        {
            lightSources = kanikamaSceneDescriptor.GetLightSources();
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
            if (colorsInternal == null || colorsInternal.Length != lightSources.Count)
            {
                colorsInternal = new Vector4[lightSources.Count];
            }
            for (var i = 0; i < lightSources.Count; i++)
            {
                colorsInternal[i] = lightSources[i].GetColorLinear();
            }

            Shader.SetGlobalVectorArray(Colors, colorsInternal);
            Shader.SetGlobalInt(Count, colorsInternal.Length);
        }
    }
}
