using UnityEngine;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(BakeryLightMesh), typeof(Renderer))]
    public class KanikamaBakeryLightMeshV2 : KanikamaLightSource
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        [Header("Baking")] [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField, HideInInspector] string gameObjectTag;
        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] float intensity;
        [SerializeField, HideInInspector] bool rendererEnable;
        [SerializeField, HideInInspector] bool bakeryLightMeshEnable;
        [SerializeField, HideInInspector] bool gameObjectActive;

        void OnValidate()
        {
            if (renderer == null) renderer = GetComponent<Renderer>();
            if (bakeryLightMesh == null) bakeryLightMesh = GetComponent<BakeryLightMesh>();
        }

        public override void Initialize()
        {
            color = bakeryLightMesh.color;
            intensity = bakeryLightMesh.intensity;
            var go = gameObject;
            gameObjectTag = go.tag;
            go.tag = "Untagged";
            rendererEnable = renderer.enabled;
            bakeryLightMeshEnable = bakeryLightMesh.enabled;
            gameObjectActive = go.activeSelf;
        }

        public override void TurnOff()
        {
            renderer.enabled = false;
            bakeryLightMesh.enabled = false;
            if (!gameObjectActive)
            {
                gameObject.SetActive(false);
            }
        }

        public override void TurnOn()
        {
            renderer.enabled = true;
            bakeryLightMesh.enabled = true;
            bakeryLightMesh.color = Color.white;
            bakeryLightMesh.intensity = 1f;
            if (!gameObjectActive)
            {
                gameObject.SetActive(true);
            }
        }

        public override void Clear()
        {
            renderer.enabled = rendererEnable;
            bakeryLightMesh.enabled = bakeryLightMeshEnable;
            bakeryLightMesh.color = color;
            bakeryLightMesh.intensity = intensity;
            gameObject.tag = gameObjectTag;
            gameObject.SetActive(gameObjectActive);
        }
#endif
        [Header("Runtime")] [SerializeField] Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] Material instance;
        [SerializeField] bool useMaterialPropertyBlock = true;

        bool useMaterialPropertyBlockInternal;

        int propertyId;
        MaterialPropertyBlock block;

        void Awake()
        {
            propertyId = KanikamaShader.PropertyToID(propertyName);
            useMaterialPropertyBlockInternal = useMaterialPropertyBlock;
            if (useMaterialPropertyBlockInternal)
            {
                block = new MaterialPropertyBlock();
            }
            else
            {
                instance = renderer.materials[materialIndex];
            }
        }

        public override Color GetLinearColor()
        {
            if (useMaterialPropertyBlockInternal)
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
