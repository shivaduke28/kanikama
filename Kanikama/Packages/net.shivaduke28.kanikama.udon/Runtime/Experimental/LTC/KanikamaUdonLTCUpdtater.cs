using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kanikama.Udon.Experimental.LTC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public sealed class KanikamaUdonLTCUpdtater : UdonSharpBehaviour
    {
        [SerializeField] Transform[] lightSources;
        [SerializeField] Renderer[] receivers;
        [SerializeField] Texture[] visibilityMaps;
        [SerializeField] Texture ltcLut0;
        [SerializeField] Texture ltcLut1;
        [SerializeField] Texture lightSourceTex;

        Vector4[] vertex0;
        Vector4[] vertex1;
        Vector4[] vertex2;
        Vector4[] vertex3;

        int ltcCount;
        int ltcVertex0;
        int ltcVertex1;
        int ltcVertex2;
        int ltcVertex3;
        int ltcTex0;
        int ltcTex1;
        int ltcLightTex0;
        int visibilityMap;

        int count;

        void Start()
        {
            ltcCount = VRCShader.PropertyToID("_Udon_LTC_Count");
            visibilityMap = VRCShader.PropertyToID("_Udon_LTC_VisibilityMap");
            ltcVertex0 = VRCShader.PropertyToID("_Udon_LTC_Vertex0");
            ltcVertex1 = VRCShader.PropertyToID("_Udon_LTC_Vertex1");
            ltcVertex2 = VRCShader.PropertyToID("_Udon_LTC_Vertex2");
            ltcVertex3 = VRCShader.PropertyToID("_Udon_LTC_Vertex3");
            ltcTex0 = VRCShader.PropertyToID("_Udon_LTC_LUT0");
            ltcTex1 = VRCShader.PropertyToID("_Udon_LTC_LUT1");
            ltcLightTex0 = VRCShader.PropertyToID("_Udon_LTC_LightTex0");

            vertex0 = new Vector4[3];
            vertex1 = new Vector4[3];
            vertex2 = new Vector4[3];
            vertex3 = new Vector4[3];

            count = Mathf.Min(3, lightSources.Length);
            VRCShader.SetGlobalInteger(ltcCount, count);
            VRCShader.SetGlobalTexture(ltcTex0, ltcLut0);
            VRCShader.SetGlobalTexture(ltcTex1, ltcLut1);
            VRCShader.SetGlobalTexture(ltcLightTex0, lightSourceTex);

            var block = new MaterialPropertyBlock();
            foreach (var r in receivers)
            {
                var i = r.lightmapIndex;
                if (i < 0 || i >= visibilityMaps.Length)
                {
                    Debug.LogWarning($"invalid lightmap index. {r.name}: {i}");
                    continue;
                }

                r.GetPropertyBlock(block);
                block.SetTexture(visibilityMap, visibilityMaps[i]);
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

            VRCShader.SetGlobalVectorArray(ltcVertex0, vertex0);
            VRCShader.SetGlobalVectorArray(ltcVertex1, vertex1);
            VRCShader.SetGlobalVectorArray(ltcVertex2, vertex2);
            VRCShader.SetGlobalVectorArray(ltcVertex3, vertex3);
        }
    }
}
