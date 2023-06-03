using UnityEngine.SceneManagement;

namespace Kanikama.Editor.Baking.Util
{
    public static class GameObjectHelper
    {
        public static T FindObjectOfType<T>()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                var child = root.GetComponentInChildren<T>();
                if (child != null)
                {
                    return child;
                }
            }
            return default;
        }
    }
}
