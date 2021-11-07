using UnityEngine;

namespace Kanikama.EditorOnly
{
    public class EditorOnlyBehaviour : MonoBehaviour
    {
        public void SetTag()
        {
            gameObject.tag = "EditorOnly";
        }
    }
}
