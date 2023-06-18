using System;
using UnityEngine;

namespace Kanikama.Application.Experimental.LTC
{
    public sealed class KanikamaLTCUpdater : MonoBehaviour
    {
        [SerializeField] Transform[] lightSources;
        [SerializeField] Renderer[] receivers;
        [SerializeField] Texture[] shadowMaps;
        [SerializeField] Texture ltc0;
        [SerializeField] Texture ltc1;
        [SerializeField] Texture lightSourceTex;

        readonly Vector4[] vertex0 = new Vector4[3];
        readonly Vector4[] vertex1 = new Vector4[3];
        readonly Vector4[] vertex2 = new Vector4[3];
        readonly Vector4[] vertex3 = new Vector4[3];

        static readonly int LtcCount = Shader.PropertyToID("_Udon_LTC_Count");
        static readonly int LtcVisibilityMap = Shader.PropertyToID("_Udon_LTC_VisibilityMap");

        static readonly int LtcVertex0 = Shader.PropertyToID("_Udon_LTC_Vertex0");
        static readonly int LtcVertex1 = Shader.PropertyToID("_Udon_LTC_Vertex1");
        static readonly int LtcVertex2 = Shader.PropertyToID("_Udon_LTC_Vertex2");
        static readonly int LtcVertex3 = Shader.PropertyToID("_Udon_LTC_Vertex3");
        static readonly int UdonLtcLut0 = Shader.PropertyToID("_Udon_LTC_LUT0");
        static readonly int UdonLtcLut1 = Shader.PropertyToID("_Udon_LTC_LUT1");
        static readonly int UdonLtcLightTex0 = Shader.PropertyToID("_Udon_LTC_LightTex0");

        int count;

        void Start()
        {
            count = Mathf.Min(3, lightSources.Length);
            Shader.SetGlobalInt(LtcCount, count);
            Shader.SetGlobalTexture(UdonLtcLut0, ltc0);
            Shader.SetGlobalTexture(UdonLtcLut1, ltc1);
            Shader.SetGlobalTexture(UdonLtcLightTex0, lightSourceTex);

            var block = new MaterialPropertyBlock();
            foreach (var r in receivers)
            {
                var i = r.lightmapIndex;
                if (i < 0 || i >= shadowMaps.Length)
                {
                    Debug.LogWarning($"invalid lightmap index. {r.name}: {i}");
                    continue;
                }

                r.GetPropertyBlock(block);
                block.SetTexture(LtcVisibilityMap, shadowMaps[i]);
                r.SetPropertyBlock(block);
            }

            UpdateVertexPositions();
        }

        void UpdateVertexPositions()
        {
            // Unity's Quad mesh
            var w = 0.5f;
            var h = 0.5f;

            for (var i = 0; i < count; i++)
            {
                var localToWorld = lightSources[i].localToWorldMatrix;

                var p0 = new Vector3(w, -h, 0);
                var p1 = new Vector3(-w, -h, 0);
                var p2 = new Vector3(-w, h, 0);
                var p3 = new Vector3(w, h, 0);

                p0 = localToWorld.MultiplyPoint3x4(p0);
                p1 = localToWorld.MultiplyPoint3x4(p1);
                p2 = localToWorld.MultiplyPoint3x4(p2);
                p3 = localToWorld.MultiplyPoint3x4(p3);

                vertex0[i] = p0;
                vertex1[i] = p1;
                vertex2[i] = p2;
                vertex3[i] = p3;
            }

            Shader.SetGlobalVectorArray(LtcVertex0, vertex0);
            Shader.SetGlobalVectorArray(LtcVertex1, vertex1);
            Shader.SetGlobalVectorArray(LtcVertex2, vertex2);
            Shader.SetGlobalVectorArray(LtcVertex3, vertex3);
        }
    }
}
