using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kanikama.Editor.Application
{
    // This is a modification of StandardShaderGUI.cs from
    // Unity built-in shader source.
    //
    // Copyright (c) 2016 Unity Technologies
    //
    // Permission is hereby granted, free of charge, to any person obtaining a copy of
    // this software and associated documentation files (the "Software"), to deal in
    // the Software without restriction, including without limitation the rights to
    // use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
    // of the Software, and to permit persons to whom the Software is furnished to do
    // so, subject to the following conditions:
    //
    // The above copyright notice and this permission notice shall be included in all
    // copies or substantial portions of the Software.
    //
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
    // FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
    // COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
    // IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
    // CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    public class KanikamaStandardGUI : ShaderGUI
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        public enum SmoothnessMapChannel
        {
            SpecularMetallicAlpha,
            AlbedoAlpha
        }

        MaterialProperty albedoColor;
        MaterialProperty albedoMap;
        MaterialProperty alphaCutoff;
        MaterialProperty blendMode;
        MaterialProperty bumpMap;

        MaterialProperty bumpScale;

        // MaterialProperty detailAlbedoMap;
        // MaterialProperty detailMask;
        // MaterialProperty detailNormalMap;
        // MaterialProperty detailNormalMapScale;
        MaterialProperty emissionColorForRendering;
        MaterialProperty emissionMap;
        MaterialProperty heightMap;
        MaterialProperty heigtMapScale;
        MaterialProperty highlights;
        bool m_FirstTimeApply = true;
        MaterialEditor m_MaterialEditor;
        WorkflowMode m_WorkflowMode = WorkflowMode.Metallic;
        MaterialProperty metallic;
        MaterialProperty metallicMap;
        MaterialProperty occlusionMap;
        MaterialProperty occlusionStrength;
        MaterialProperty reflections;
        MaterialProperty smoothness;
        MaterialProperty smoothnessMapChannel;

        MaterialProperty smoothnessScale;
        // MaterialProperty specularColor;
        // MaterialProperty specularMap;
        // MaterialProperty uvSetSecondary;

        MaterialProperty kanikamaMode;
        MaterialProperty kanikamaDirectionalSpecular;
        MaterialProperty kanikamaLTC;

        void FindKanikamaProperties(MaterialProperty[] properties)
        {
            kanikamaMode = FindProperty("_Kanikama_Mode", properties);
            kanikamaDirectionalSpecular = FindProperty("_Kanikama_Directional_Specular", properties);
            kanikamaLTC = FindProperty("_Kanikama_LTC", properties);
        }

        void KanikamaPropertiesGUI(MaterialEditor materialEditor)
        {
            GUILayout.Label("Kanikama", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(kanikamaMode, "Kanikama Mode");
            materialEditor.ShaderProperty(kanikamaDirectionalSpecular, "Kanikama Directional Specular");
            materialEditor.ShaderProperty(kanikamaLTC, "Kanikama LTC");
        }

        public void FindProperties(MaterialProperty[] props)
        {
            blendMode = FindProperty("_Mode", props);
            albedoMap = FindProperty("_MainTex", props);
            albedoColor = FindProperty("_Color", props);
            alphaCutoff = FindProperty("_Cutoff", props);
            // specularMap = FindProperty("_SpecGlossMap", props, false);
            // specularColor = FindProperty("_SpecColor", props, false);
            metallicMap = FindProperty("_MetallicGlossMap", props, false);
            metallic = FindProperty("_Metallic", props, false);
            // m_WorkflowMode = specularMap == null || specularColor == null
            //     ? metallicMap == null || metallic == null ? WorkflowMode.Dielectric : WorkflowMode.Metallic
            //     : WorkflowMode.Specular;
            smoothness = FindProperty("_Glossiness", props);
            smoothnessScale = FindProperty("_GlossMapScale", props, false);
            smoothnessMapChannel = FindProperty("_SmoothnessTextureChannel", props, false);
            highlights = FindProperty("_SpecularHighlights", props, false);
            reflections = FindProperty("_GlossyReflections", props, false);
            bumpScale = FindProperty("_BumpScale", props);
            bumpMap = FindProperty("_BumpMap", props);
            heigtMapScale = FindProperty("_Parallax", props);
            heightMap = FindProperty("_ParallaxMap", props);
            occlusionStrength = FindProperty("_OcclusionStrength", props);
            occlusionMap = FindProperty("_OcclusionMap", props);
            emissionColorForRendering = FindProperty("_EmissionColor", props);
            emissionMap = FindProperty("_EmissionMap", props);
            // detailMask = FindProperty("_DetailMask", props);
            // detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
            // detailNormalMapScale = FindProperty("_DetailNormalMapScale", props);
            // detailNormalMap = FindProperty("_DetailNormalMap", props);
            // uvSetSecondary = FindProperty("_UVSec", props);

            FindKanikamaProperties(props);
        }


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props);
            m_MaterialEditor = materialEditor;
            var target = materialEditor.target as Material;
            if (m_FirstTimeApply)
            {
                MaterialChanged(target, m_WorkflowMode, false);
                m_FirstTimeApply = false;
            }
            ShaderPropertiesGUI(target);
            KanikamaPropertiesGUI(materialEditor);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            EditorGUIUtility.labelWidth = 0.0f;
            EditorGUI.BeginChangeCheck();
            var overrideRenderQueue = BlendModePopup();
            GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
            DoAlbedoArea(material);
            DoSpecularMetallicArea();
            DoNormalArea();
            m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap, heightMap.textureValue != null ? heigtMapScale : null);
            m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
            // m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
            DoEmissionArea(material);
            EditorGUI.BeginChangeCheck();
            m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
            if (EditorGUI.EndChangeCheck())
                emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset;
            EditorGUILayout.Space();
            // GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
            // m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
            // m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
            // m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
            // m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);
            // GUILayout.Label(Styles.forwardText, EditorStyles.boldLabel);
            // if (highlights != null)
            //     m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
            // if (reflections != null)
            //     m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);
            // EditorGUILayout.Space();
            GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);
            m_MaterialEditor.RenderQueueField();
            if (EditorGUI.EndChangeCheck())
                foreach (Material target in blendMode.targets)
                    MaterialChanged(target, m_WorkflowMode, overrideRenderQueue);
            m_MaterialEditor.EnableInstancingField();
            m_MaterialEditor.DoubleSidedGIField();
        }

        internal void DetermineWorkflow(MaterialProperty[] props)
        {
            if (FindProperty("_SpecGlossMap", props, false) != null && FindProperty("_SpecColor", props, false) != null)
                m_WorkflowMode = WorkflowMode.Specular;
            else if (FindProperty("_MetallicGlossMap", props, false) != null && FindProperty("_Metallic", props, false) != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            else
                m_WorkflowMode = WorkflowMode.Dielectric;
        }

        public override void AssignNewShaderToMaterial(
            Material material,
            Shader oldShader,
            Shader newShader)
        {
            if (material.HasProperty("_Emission"))
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode) material.GetFloat("_Mode"), true);
            }
            else
            {
                var blendMode = BlendMode.Opaque;
                if (oldShader.name.Contains("/Transparent/Cutout/"))
                    blendMode = BlendMode.Cutout;
                else if (oldShader.name.Contains("/Transparent/"))
                    blendMode = BlendMode.Fade;
                material.SetFloat("_Mode", (float) blendMode);
                DetermineWorkflow(MaterialEditor.GetMaterialProperties(new Material[1]
                {
                    material
                }));
                MaterialChanged(material, m_WorkflowMode, true);
            }
        }

        bool BlendModePopup()
        {
            EditorGUI.showMixedValue = this.blendMode.hasMixedValue;
            var floatValue = (BlendMode) this.blendMode.floatValue;
            EditorGUI.BeginChangeCheck();
            var blendMode = (BlendMode) EditorGUILayout.Popup(Styles.renderingMode, (int) floatValue, Styles.blendNames);
            var flag = EditorGUI.EndChangeCheck();
            if (flag)
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                this.blendMode.floatValue = (float) blendMode;
            }
            EditorGUI.showMixedValue = false;
            return flag;
        }

        void DoNormalArea()
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
        }

        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);
            if ((int) material.GetFloat("_Mode") != 1)
                return;
            m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, 3);
        }

        void DoEmissionArea(Material material)
        {
            if (!m_MaterialEditor.EmissionEnabledProperty())
                return;
            var flag = emissionMap.textureValue != null;
            m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);
            var maxColorComponent = emissionColorForRendering.colorValue.maxColorComponent;
            if (emissionMap.textureValue != null && !flag && maxColorComponent <= 0.0)
                emissionColorForRendering.colorValue = Color.white;
            m_MaterialEditor.LightmapEmissionFlagsProperty(2, true);
        }

        void DoSpecularMetallicArea()
        {
            var flag1 = false;
            if (m_WorkflowMode == WorkflowMode.Specular)
            {
                // flag1 = specularMap.textureValue != null;
                // m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap, flag1 ? null : specularColor);
            }
            else if (m_WorkflowMode == WorkflowMode.Metallic)
            {
                flag1 = metallicMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, metallic);
            }
            var flag2 = flag1;
            // if (smoothnessMapChannel != null && (int) smoothnessMapChannel.floatValue == 1)
            //     flag2 = true;
            var labelIndent1 = 2;
            // m_MaterialEditor.ShaderProperty(flag2 ? smoothnessScale : smoothness, flag2 ? Styles.smoothnessScaleText : Styles.smoothnessText, labelIndent1);
            m_MaterialEditor.ShaderProperty(smoothness, Styles.smoothnessText, labelIndent1);
            // var labelIndent2 = labelIndent1 + 1;
            // if (smoothnessMapChannel == null)
            //     return;
            // m_MaterialEditor.ShaderProperty(smoothnessMapChannel, Styles.smoothnessMapChannelText, labelIndent2);
        }

        public static void SetupMaterialWithBlendMode(
            Material material,
            BlendMode blendMode,
            bool overrideRenderQueue)
        {
            var num1 = -1;
            var num2 = 5000;
            var num3 = -1;
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    num1 = -1;
                    num2 = 2449;
                    num3 = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    num1 = 2450;
                    num2 = 2500;
                    num3 = 2450;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", 5);
                    material.SetInt("_DstBlend", 10);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    num1 = 2501;
                    num2 = 3999;
                    num3 = 3000;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 10);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    num1 = 2501;
                    num2 = 3999;
                    num3 = 3000;
                    break;
            }
            if (!overrideRenderQueue && material.renderQueue >= num1 && material.renderQueue <= num2)
                return;
            if (!overrideRenderQueue)
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
                    "Render queue value outside of the allowed range ({0} - {1}) for selected Blend mode, resetting render queue to default", num1, num2);
            material.renderQueue = num3;
        }

        static SmoothnessMapChannel GetSmoothnessMapChannel(
            Material material)
        {
            return (int) material.GetFloat("_SmoothnessTextureChannel") == 1 ? SmoothnessMapChannel.AlbedoAlpha : SmoothnessMapChannel.SpecularMetallicAlpha;
        }

        static void SetMaterialKeywords(
            Material material,
            WorkflowMode workflowMode)
        {
            // SetKeyword(material, "_NORMALMAP", (bool) (Object) material.GetTexture("_BumpMap") || (bool) (Object) material.GetTexture("_DetailNormalMap"));
            switch (workflowMode)
            {
                // case WorkflowMode.Specular:
                //     SetKeyword(material, "_SPECGLOSSMAP", (bool) (Object) material.GetTexture("_SpecGlossMap"));
                //     break;
                case WorkflowMode.Metallic:
                    SetKeyword(material, "_METALLICGLOSSMAP", (bool) (Object) material.GetTexture("_MetallicGlossMap"));
                    break;
            }
            // SetKeyword(material, "_PARALLAXMAP", (bool) (Object) material.GetTexture("_ParallaxMap"));
            // SetKeyword(material, "_DETAIL_MULX2", (bool) (Object) material.GetTexture("_DetailAlbedoMap") || (bool) (Object) material.GetTexture("_DetailNormalMap"));
            MaterialEditor.FixupEmissiveFlag(material);
            var state = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == MaterialGlobalIlluminationFlags.None;
            SetKeyword(material, "_EMISSION", state);
            if (!material.HasProperty("_SmoothnessTextureChannel")) return;
            SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha);
        }

        static void MaterialChanged(
            Material material,
            WorkflowMode workflowMode,
            bool overrideRenderQueue)
        {
            SetupMaterialWithBlendMode(material, (BlendMode) material.GetFloat("_Mode"), overrideRenderQueue);
            SetMaterialKeywords(material, workflowMode);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }

        enum WorkflowMode
        {
            Specular,
            Metallic,
            Dielectric
        }

        static class Styles
        {
            // public static readonly GUIContent uvSetLabel = EditorGUIUtility.TrTextContent("UV Set");
            public static readonly GUIContent albedoText = EditorGUIUtility.TrTextContent("Albedo", "Albedo (RGB) and Transparency (A)");
            public static readonly GUIContent alphaCutoffText = EditorGUIUtility.TrTextContent("Alpha Cutoff", "Threshold for alpha cutoff");
            // public static readonly GUIContent specularMapText = EditorGUIUtility.TrTextContent("Specular", "Specular (RGB) and Smoothness (A)");
            public static readonly GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic", "Metallic (R) and Smoothness (A)");
            public static readonly GUIContent smoothnessText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness value");
            public static readonly GUIContent smoothnessScaleText = EditorGUIUtility.TrTextContent("Smoothness", "Smoothness scale factor");
            public static readonly GUIContent smoothnessMapChannelText = EditorGUIUtility.TrTextContent("Source", "Smoothness texture and channel");
            public static readonly GUIContent highlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "Specular Highlights");
            public static readonly GUIContent reflectionsText = EditorGUIUtility.TrTextContent("Reflections", "Glossy Reflections");
            public static readonly GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
            public static readonly GUIContent heightMapText = EditorGUIUtility.TrTextContent("Height Map", "Height Map (G)");
            public static readonly GUIContent occlusionText = EditorGUIUtility.TrTextContent("Occlusion", "Occlusion (G)");
            public static readonly GUIContent emissionText = EditorGUIUtility.TrTextContent("Color", "Emission (RGB)");
            // public static readonly GUIContent detailMaskText = EditorGUIUtility.TrTextContent("Detail Mask", "Mask for Secondary Maps (A)");
            // public static readonly GUIContent detailAlbedoText = EditorGUIUtility.TrTextContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
            // public static readonly GUIContent detailNormalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
            public static readonly string primaryMapsText = "Main Maps";
            public static readonly string secondaryMapsText = "Secondary Maps";
            public static readonly string forwardText = "Forward Rendering Options";
            public static readonly string renderingMode = "Rendering Mode";
            public static readonly string advancedText = "Advanced Options";
            public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
        }
    }
}
