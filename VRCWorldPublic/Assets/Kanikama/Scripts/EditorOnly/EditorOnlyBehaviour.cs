using UnityEngine;

namespace Kanikama.EditorOnly
{
    public class EditorOnlyBehaviour : MonoBehaviour
    {
        private void OnValidate()
        {
            gameObject.tag = "EditorOnly";
        }
    }
}
