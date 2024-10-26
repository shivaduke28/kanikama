using UnityEngine;

namespace Kanikama.Bakery
{
    [RequireComponent(typeof(Renderer), typeof(BakeryLightMesh))]
    public sealed class KanikamaBakeryLightMesh : BakeTarget
    {
        [SerializeField] new Renderer renderer;
        [SerializeField] BakeryLightMesh bakeryLightMesh;
        [SerializeField] string gameObjectTag;
        [SerializeField] Color color;
        [SerializeField] float intensity;
        [SerializeField] bool rendererEnable;
        [SerializeField] bool bakeryLightMeshEnable;
        [SerializeField] bool gameObjectActive;

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
    }
}
