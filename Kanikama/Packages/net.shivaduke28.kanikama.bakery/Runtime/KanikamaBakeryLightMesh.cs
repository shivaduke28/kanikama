using UnityEngine;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(BakeryLightMesh), typeof(Renderer))]
    public class KanikamaBakeryLightMesh : KanikamaLightSource
    {
        [Header("Baking")]
        [SerializeField, HideInInspector]
        string gameObjectTag;

        [SerializeField, HideInInspector] Color color;
        [SerializeField, HideInInspector] float intensity;
        [SerializeField, HideInInspector] bool rendererEnable;
        [SerializeField, HideInInspector] bool bakeryLightMeshEnable;
        [SerializeField, HideInInspector] bool gameObjectActive;

        [Header("Runtime")] [SerializeField] new Renderer renderer;
        [SerializeField] int materialIndex;
        [SerializeField] string propertyName = "_EmissionColor";
        [SerializeField] Material instance;
        [SerializeField] bool useMaterialPropertyBlock = true;

        bool useMaterialPropertyBlockInternal;

        int propertyId;
        MaterialPropertyBlock block;


#if !COMPILER_UDONSHARP && UNITY_EDITOR
        BakeryLightMesh BakeryLightMesh => GetComponent<BakeryLightMesh>();

        void Reset()
        {
            renderer = GetComponent<Renderer>();
        }

        public override void Initialize()
        {
            color = BakeryLightMesh.color;
            intensity = BakeryLightMesh.intensity;
            var go = gameObject;
            gameObjectTag = go.tag;
            go.tag = "Untagged";
            rendererEnable = renderer.enabled;
            bakeryLightMeshEnable = BakeryLightMesh.enabled;
            gameObjectActive = go.activeSelf;
        }

        public override void TurnOff()
        {
            renderer.enabled = false;
            BakeryLightMesh.enabled = false;
            if (!gameObjectActive)
            {
                gameObject.SetActive(false);
            }
        }

        public override void TurnOn()
        {
            renderer.enabled = true;
            BakeryLightMesh.enabled = true;
            BakeryLightMesh.color = Color.white;
            BakeryLightMesh.intensity = 1f;
            if (!gameObjectActive)
            {
                gameObject.SetActive(true);
            }
        }

        public override void Clear()
        {
            renderer.enabled = rendererEnable;
            BakeryLightMesh.enabled = bakeryLightMeshEnable;
            BakeryLightMesh.color = color;
            BakeryLightMesh.intensity = intensity;
            gameObject.tag = gameObjectTag;
            gameObject.SetActive(gameObjectActive);
        }
#endif

        void Start()
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
