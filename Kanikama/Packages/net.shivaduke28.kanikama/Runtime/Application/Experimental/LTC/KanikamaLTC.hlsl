/*
https://github.com/selfshadow/ltc_code

Copyright (c) 2017, Eric Heitz, Jonathan Dupuy, Stephen Hill and David Neubelt.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* If you use (or adapt) the source code in your own work, please include a 
  reference to the paper:

  Real-Time Polygonal-Light Shading with Linearly Transformed Cosines.
  Eric Heitz, Jonathan Dupuy, Stephen Hill and David Neubelt.
  ACM Transactions on Graphics (Proceedings of ACM SIGGRAPH 2016) 35(4), 2016.
  Project page: https://eheitzresearch.wordpress.com/415-2/

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#ifndef KANIKAMA_LTC_INCLUDED
#define KANIKAMA_LTC_INCLUDED

#define MAX_LIGHT_SOURCE_COUNT 3

// Global
float3 _Udon_LTC_Vertex0[MAX_LIGHT_SOURCE_COUNT];
float3 _Udon_LTC_Vertex1[MAX_LIGHT_SOURCE_COUNT];
float3 _Udon_LTC_Vertex2[MAX_LIGHT_SOURCE_COUNT];
float3 _Udon_LTC_Vertex3[MAX_LIGHT_SOURCE_COUNT];
// 0 ~ 3
int _Udon_LTC_Count;

// const
// TODO use trilinear sampler
sampler2D _LightSourceTex0;

// const
sampler2D _LTC_1;
sampler2D _LTC_2;

// Per Renderer Data
sampler2D _Udon_LTC_ShadowMap;

static const float LUT_SIZE = 64;
static const float LUT_SCALE = (LUT_SIZE - 1.0) / LUT_SIZE;
static const float LUT_BIAS = 0.5 / LUT_SIZE;

float3 IntegrateEdgeVec(float3 v1, float3 v2)
{
    float x = dot(v1, v2);
    float y = abs(x);

    float a = 0.8543985 + (0.4965155 + 0.0145206 * y) * y;
    float b = 3.4175940 + (4.1616724 + y) * y;
    float v = a / b;

    float theta_sintheta = (x > 0.0) ? v : 0.5 * rsqrt(max(1.0 - x * x, 1e-7)) - v;

    return cross(v1, v2) * theta_sintheta;
}

float SquareSDF(float2 uv)
{
    uv -= 0.5;
    float2 st = abs(uv) - 0.5;
    return max(st.x, st.y);
}

float GaussianKernel(in float x, in float sigma)
{
    float s = 1/ sigma;
    // 1/sqrt(2 * PI) = 0.39894
    return 0.39894 * exp(-0.5 * x * x * s * s) * s;
}

float GaussianInv(float y, float sigma)
{
    // sqrt(2 * PI) = 2.50662
    return sigma * sqrt(-2 * log(2.50662 * sigma * y));
}

half3 FetchTexture(sampler2D tex, float3 p0, float3 p1, float3 p2)
{
    // uv
    float3 V1 = p0 - p1;
    float3 V2 = p2 - p1;
    float3 planeOrtho = cross(V1, V2);
    float planeAreaSquared = dot(planeOrtho, planeOrtho);
    float planeDistxPlaneArea = dot(planeOrtho, p1);
    float3 P = planeDistxPlaneArea * planeOrtho / planeAreaSquared - p1;

    float dot_V1_V2 = dot(V1, V2);
    float inv_dot_V1_V1 = 1 / dot(V1, V1);
    float3 V2_ = V2 - V1 * dot_V1_V2 * inv_dot_V1_V1;
    float2 uv;
    uv.y = dot(V2_, P) / dot(V2_, V2_);
    uv.x = dot(V1, P) * inv_dot_V1_V1 - dot_V1_V2 * inv_dot_V1_V1 * uv.y;

    float sigma = abs(planeDistxPlaneArea) / pow(planeAreaSquared, 0.75);
    float add = max(0, SquareSDF(uv));
    sigma += add;

    // Approximate Gaussian function by step functions.
    // Texture's Filter Mode should be Trilinear. 
    float y0 = GaussianKernel(0, sigma);
    float y1 = y0 * 0.75;
    float x1 = GaussianInv(y1, sigma);
    float y2 = y0 * 0.5;
    float x2 = GaussianInv(y2, sigma);
    float y3 = y0 * 0.25;
    float x3 = GaussianInv(y3, sigma);

    half4 col = 0;

    float2 dx = float2(0.5, 0);
    float2 dy = float2(0, 0.5);

    col += tex2Dgrad(tex, uv, dx * x3, dy * x3) * 0.333;
    col += tex2Dgrad(tex, uv, dx * x2, dy * x2) * 0.333;
    col += tex2Dgrad(tex, uv, dx * x1, dy * x1) * 0.333;
    return col;
}

// Minv must be mul(Minv, float3x3(v tangent, bitangent, normal)))
half3 LTCEvaluate(float3 pos, float3x3 Minv, float3 points[4], sampler2D tex)
{
    float3 dir = pos - points[0];
    float3 lightNormal = cross(points[1] - points[0], points[3] - points[0]);
    bool behind = dot(dir, lightNormal) < 0.0;

    float sum;
    if (behind)
    {
        return half3(0, 0, 0);
    }
    float3 vsum = 0;
    float3 p0 = mul(Minv, points[0] - pos);
    float3 p1 = mul(Minv, points[1] - pos);
    float3 p2 = mul(Minv, points[2] - pos);
    float3 p3 = mul(Minv, points[3] - pos);
    float3 l0 = normalize(p0);
    float3 l1 = normalize(p1);
    float3 l2 = normalize(p2);
    float3 l3 = normalize(p3);

    vsum += IntegrateEdgeVec(l0, l1);
    vsum += IntegrateEdgeVec(l1, l2);
    vsum += IntegrateEdgeVec(l2, l3);
    vsum += IntegrateEdgeVec(l3, l0);

    float len = length(vsum);
    float z = vsum.z / len;

    float2 uv2 = float2(z * 0.5 + 0.5, len);
    uv2 = uv2 * LUT_SCALE + LUT_BIAS;

    float4 ltc2_tex = tex2D(_LTC_2, uv2);
    float scale = ltc2_tex.w;
    sum = len * scale;

    half3 texColor = FetchTexture(tex, p0, p1, p2);
    return texColor * sum;
}

half3 LTCEvaluateNoTexture(float3 pos, float3x3 Minv, float3 points[4])
{
    float3 dir = pos - points[0];
    float3 lightNormal = cross(points[1] - points[0], points[3] - points[0]);
    bool behind = dot(dir, lightNormal) < 0.0;

    float sum;
    if (behind)
    {
        return half3(0, 0, 0);
    }
    float3 vsum = 0;
    float3 p0 = mul(Minv, points[0] - pos);
    float3 p1 = mul(Minv, points[1] - pos);
    float3 p2 = mul(Minv, points[2] - pos);
    float3 p3 = mul(Minv, points[3] - pos);
    float3 l0 = normalize(p0);
    float3 l1 = normalize(p1);
    float3 l2 = normalize(p2);
    float3 l3 = normalize(p3);

    vsum += IntegrateEdgeVec(l0, l1);
    vsum += IntegrateEdgeVec(l1, l2);
    vsum += IntegrateEdgeVec(l2, l3);
    vsum += IntegrateEdgeVec(l3, l0);

    float len = length(vsum);
    float z = vsum.z / len;

    float2 uv2 = float2(z * 0.5 + 0.5, len);
    uv2 = uv2 * LUT_SCALE + LUT_BIAS;

    float4 ltc2_tex = tex2D(_LTC_2, uv2);
    float scale = ltc2_tex.w;
    sum = len * scale;
    return half3(sum, sum, sum);
}

struct LTCData
{
    float3x3 Minv;
    float2 BRDFParam;
};

LTCData LTCSetup(float ndotv, float perceptualRoughness)
{
    LTCData o;
    float2 uv = float2(perceptualRoughness, sqrt(1.0 - ndotv));
    uv = uv * LUT_SCALE + LUT_BIAS;
    float4 t1 = tex2D(_LTC_1, uv);
    float4 t2 = tex2D(_LTC_2, uv);
    o.Minv = float3x3(
        t1.x, 0, t1.z,
        0, 1, 0,
        t1.y, 0, t1.w);
    o.BRDFParam = t2.xy;
    return o;
}

void KanikamaLTCSpecular(float3 position, half3 normal, half3 view, half perceptualRoughness, float2 lightmapUV,
                         half occlusion,
                         half3 specColor,
                         out half3 specular)
{
    float ndotv = saturate(dot(normal, view));
    LTCData data = LTCSetup(ndotv, perceptualRoughness);
    half3 viewTangent = normalize(view - ndotv * normal);
    half3 viewBitangent = cross(viewTangent, normal);
    float3x3 orth = float3x3(viewTangent, viewBitangent, normal);
    float3x3 Minv = mul(data.Minv, orth);
    float2 ltcParam = data.BRDFParam;

    float3 shadow = tex2D(_Udon_LTC_ShadowMap, lightmapUV).rgb;
    float3 points[4];
    specular = 0;

    for (int i = 0; i < _Udon_LTC_Count; i++)
    {
        points[0] = _Udon_LTC_Vertex0[i];
        points[1] = _Udon_LTC_Vertex1[i];
        points[2] = _Udon_LTC_Vertex2[i];
        points[3] = _Udon_LTC_Vertex3[i];
        half3 ltcDiff = LTCEvaluateNoTexture(position, orth, points);
        half3 ltcSpec = LTCEvaluate(position, Minv, points, _LightSourceTex0);
        ltcSpec *= ltcParam.x + (1.0 - specColor) * ltcParam.y;
        // fake shadowing using lightmap
        ltcSpec *= saturate(shadow[i] / max(0.001, Luminance(ltcDiff)));
        specular += ltcSpec;
    }
    specular *= occlusion;
}

#endif
