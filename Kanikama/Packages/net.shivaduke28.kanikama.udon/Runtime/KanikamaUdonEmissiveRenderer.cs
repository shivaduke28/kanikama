using UnityEngine;
using VRC.SDKBase;

namespace Kanikama.Udon
{
    [RequireComponent(typeof(Renderer))]
    public class KanikamaUdonEmissiveRenderer : KanikamaUdonLightSource
    {
        [SerializeField, NonNull] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] Material instance;
        [SerializeField] bool useMaterialPropertyBlock = true;

#if !COMPILER_UDONSHARP
        [SerializeField] bool useStandardShader;
        [SerializeField] string gameObjectTag;

        public override void Initialize()
        {
            var go = gameObject;
            gameObjectTag = go.tag;
            go.tag = "Untagged";
            if (!TryGetComponent<Components.RendererMaterialHolder>(out var holder))
            {
                holder = go.AddComponent<Components.RendererMaterialHolder>();
            }

            if (useStandardShader)
            {
                var mat = holder.GetMaterial(materialIndex);
                mat.shader = Shader.Find("Standard");
                mat.SetColor(propertyName, Color.white);
                mat.AddBakedEmissiveFlag();
            }
        }

        public override void TurnOff()
        {
            gameObject.tag = gameObjectTag;
            var holder = GetComponent<Components.RendererMaterialHolder>();
            holder.GetMaterial(materialIndex).RemoveBakedEmissiveFlag();
        }

        public override void TurnOn()
        {
            var holder = GetComponent<Components.RendererMaterialHolder>();
            var mat = holder.GetMaterial(materialIndex);
            mat.SetColor(propertyName, Color.white);
            mat.AddBakedEmissiveFlag();
            SelectionUtility.SetActiveObject(mat);
        }

        public override void Clear()
        {
            gameObject.tag = gameObjectTag;
            if (TryGetComponent<Components.RendererMaterialHolder>(out var holder))
            {
                holder.Clear();
                holder.DestroySafely();
            }
        }

        void OnValidate()
        {
            if (renderer == null)
            {
                renderer = GetComponent<Renderer>();
            }
        }
#endif

        int propertyId;
        MaterialPropertyBlock block;
        bool initialized;

        void Start()
        {
            propertyId = VRCShader.PropertyToID(propertyName);
            if (useMaterialPropertyBlock)
            {
                block = new MaterialPropertyBlock();
            }
            else
            {
                instance = renderer.materials[materialIndex];
            }
            initialized = true;
        }


        public override Color GetLinearColor()
        {
            if (!gameObject.activeSelf || !initialized) return Color.black;
            if (useMaterialPropertyBlock)
            {
                renderer.GetPropertyBlock(block);
                return block.GetColor(propertyId).linear;
            }
            else
            {
                return instance.GetColor(propertyId).linear;
            }
        }

        void OnDestroy()
        {
            if (instance != null)
            {
                Destroy(instance);
            }
        }
    }
}
