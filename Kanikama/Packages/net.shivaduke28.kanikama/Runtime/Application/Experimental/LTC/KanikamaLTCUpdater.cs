using System;
using UnityEngine;

namespace Kanikama.Application.Experimental.LTC
{
    public sealed class KanikamaLTCUpdater : MonoBehaviour
    {
        [SerializeField] Transform lightSource;
        [SerializeField] float width = 0.5f;
        [SerializeField] float height = 0.5f;

        Vector4 vertex0;
        Vector4 vertex1;
        Vector4 vertex2;
        Vector4 vertex3;

        static readonly int LtcVertex0 = Shader.PropertyToID("_LTC_Vertex0");
        static readonly int LtcVertex1 = Shader.PropertyToID("_LTC_Vertex1");
        static readonly int LtcVertex2 = Shader.PropertyToID("_LTC_Vertex2");
        static readonly int LtcVertex3 = Shader.PropertyToID("_LTC_Vertex3");

        void LateUpdate()
        {
            // Quadの場合
            var w = width;
            var h = height;

            var localToWorld = lightSource.localToWorldMatrix;

            var p0 = new Vector3(w, -h, 0);
            var p1 = new Vector3(-w, -h, 0);
            var p2 = new Vector3(-w, h, 0);
            var p3 = new Vector3(w, h, 0);

            p0 = localToWorld.MultiplyPoint3x4(p0);
            p1 = localToWorld.MultiplyPoint3x4(p1);
            p2 = localToWorld.MultiplyPoint3x4(p2);
            p3 = localToWorld.MultiplyPoint3x4(p3);

            vertex0 = p0;
            vertex1 = p1;
            vertex2 = p2;
            vertex3 = p3;

            Shader.SetGlobalVector(LtcVertex0, vertex0);
            Shader.SetGlobalVector(LtcVertex1, vertex1);
            Shader.SetGlobalVector(LtcVertex2, vertex2);
            Shader.SetGlobalVector(LtcVertex3, vertex3);
        }
    }
}
