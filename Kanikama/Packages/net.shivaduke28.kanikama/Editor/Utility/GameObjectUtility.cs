using System.Linq;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{
    public static class GameObjectUtility
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

        public static T[] GetComponentsInScene<T>(bool includeInactive)
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(includeInactive))
                .ToArray();
        }
    }
}
