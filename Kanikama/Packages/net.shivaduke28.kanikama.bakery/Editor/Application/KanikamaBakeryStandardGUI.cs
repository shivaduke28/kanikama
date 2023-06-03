﻿using UnityEditor;
using UnityEngine;

namespace Kanikama.Bakery.Editor.Application
{
    public class KanikamaBakeryStandardGUI : BakeryShaderGUI
    {
        MaterialProperty kanikamaMode;
        MaterialProperty kanikamaDirectionalSpecular;
        MaterialProperty kanikamaBakerySHNonlinear;

        void FindKanikamaProperties(MaterialProperty[] properties)
        {
            kanikamaMode = FindProperty("_Kanikama_Mode", properties);
            kanikamaDirectionalSpecular = FindProperty("_Kanikama_Directional_Specular", properties);
            kanikamaBakerySHNonlinear = FindProperty("_Kanikama_Bakery_SHNonlinear", properties);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            base.OnGUI(materialEditor, props);
            FindKanikamaProperties(props);
            KanikamaPropertiesGUI(materialEditor);
        }

        void KanikamaPropertiesGUI(MaterialEditor materialEditor)
        {
            GUILayout.Label("Kanikama", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(kanikamaMode, "Kanikama Mode");
            materialEditor.ShaderProperty(kanikamaDirectionalSpecular, "Kanikama Directional Specular");
            materialEditor.ShaderProperty(kanikamaBakerySHNonlinear, "Kanikama Bakery NonLinear SH");
        }
    }
}
