using UnityEditor;
using UnityEngine;
using Kanikama.EditorOnly;

namespace Kanikama.Editor
{
    [CreateAssetMenu(menuName = "Kanikama/Settings", fileName = "KanikamaSettings")]
    public class KanikamaSettings : ScriptableObject
    {
        [ReadOnly] [SerializeField] private SceneAsset sceneAsset;
        [ReadOnly] public bool directionalMode;
        [ReadOnly] public bool createRenderTexture;
        [ReadOnly] public bool createCustomRenderTexture;
        public SceneAsset SceneAsset => sceneAsset;

        public void Initialize(SceneAsset sceneAsset, bool directionalMode)
        {
            this.sceneAsset = sceneAsset;
            this.directionalMode = directionalMode;
        }
    }
}