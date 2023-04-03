using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Kanikama.Core.Editor.LightSources
{
    public sealed class SceneGIContext
    {
        // TODO: GlobalObjectIdに乗り換えたい
        public List<LightReference> LightReferences;
        public List<EmissiveRendererReference> EmissiveRendererReferences;
        public List<ComponentReference<LightProbeGroup>> LightProbeGroups;
        public List<ComponentReference<ReflectionProbe>> ReflectionProbes;
        public AmbientLight AmbientLight;

        public void TurnOff()
        {
            foreach (var reference in LightReferences)
            {
                reference.TurnOff();
            }

            foreach (var reference in EmissiveRendererReferences)
            {
                reference.TurnOff();
            }

            AmbientLight.TurnOff();
        }

        public void DisableLightProbes()
        {
            foreach (var reference in LightProbeGroups)
            {
                reference.Value.enabled = false;
            }
        }

        public void DisableReflectionProbes()
        {
            foreach (var reference in ReflectionProbes)
            {
                reference.Value.enabled = false;
            }
        }
    }

    public sealed class LightReference
    {
        public ComponentReference<Light> Reference { get; }
        readonly float intensity;

        public LightReference(Light light)
        {
            Reference = new ComponentReference<Light>(light);
            intensity = light.intensity;
        }

        public static bool IsContributeGI(Light light)
        {
            return light.isActiveAndEnabled && (light.lightmapBakeType & LightmapBakeType.Realtime) == 0;
        }

        public void TurnOff()
        {
            Reference.Value.intensity = 0;
        }

        public void Revert()
        {
            Reference.Value.intensity = intensity;
        }
    }

    public sealed class EmissiveRendererReference
    {
        public ComponentReference<Renderer> Reference { get; }
        readonly Material[] sharedMaterials;
        readonly Material[] tempMaterials;

        public EmissiveRendererReference(Renderer renderer)
        {
            Reference = new ComponentReference<Renderer>(renderer);
            sharedMaterials = renderer.sharedMaterials;
            var temp = sharedMaterials.Select(Object.Instantiate).ToArray();
            foreach (var mat in temp)
            {
                MaterialUtility.RemoveBakedEmissiveFlag(mat);
            }
            tempMaterials = temp.ToArray();
        }

        public void TurnOff()
        {
            Reference.Value.sharedMaterials = tempMaterials;
        }

        public void Revert()
        {
            Reference.Value.sharedMaterials = sharedMaterials;
            foreach (var mat in tempMaterials)
            {
                if (mat != null) Object.DestroyImmediate(mat);
            }
        }

        public static bool IsContributeGI(Renderer renderer)
        {
            if (!renderer.enabled) return false;
            var flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
            return flags.HasFlag(StaticEditorFlags.ContributeGI) && renderer.sharedMaterials.Any(MaterialUtility.IsContributeGI);
        }
    }

    public sealed class AmbientLight
    {
        float ambientIntensity;
        Color ambientLight;
        Color ambientSkyColor;
        Color ambientGroundColor;
        Color ambientEquatorColor;
        AmbientMode ambientMode;

        public float Intensity
        {
            get => RenderSettings.ambientIntensity;
            set => RenderSettings.ambientIntensity = value;
        }

        public Color Light
        {
            get => RenderSettings.ambientLight;
            set => RenderSettings.ambientLight = value;
        }

        public Color SkyColor
        {
            get => RenderSettings.ambientSkyColor;
            set => RenderSettings.ambientSkyColor = value;
        }

        public Color EquatorColor
        {
            get => RenderSettings.ambientEquatorColor;
            set => RenderSettings.ambientEquatorColor = value;
        }

        public Color GroundColor
        {
            get => RenderSettings.ambientGroundColor;
            set => RenderSettings.ambientGroundColor = value;
        }

        public AmbientMode Mode
        {
            get => RenderSettings.ambientMode;
            set => RenderSettings.ambientMode = value;
        }

        public void CacheCurrentRenderSettings()
        {
            ambientIntensity = RenderSettings.ambientIntensity;
            ambientLight = RenderSettings.ambientLight;
            ambientSkyColor = RenderSettings.ambientSkyColor;
            ambientEquatorColor = RenderSettings.ambientEquatorColor;
            ambientGroundColor = RenderSettings.ambientGroundColor;
        }

        public AmbientLight()
        {
            CacheCurrentRenderSettings();
        }

        public void TurnOff()
        {
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientSkyColor = Color.black;
            RenderSettings.ambientEquatorColor = Color.black;
            RenderSettings.ambientGroundColor = Color.black;
        }

        public void Revert()
        {
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
        }
    }
}
