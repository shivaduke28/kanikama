using System.Linq;
using System.Text;
using UnityEngine;

namespace Kanikama.Baking
{
    public class ObjectReference<T> where T : Component
    {
        T value;
        readonly string rootName;
        readonly string path; // relative to root

        public ObjectReference(T value)
        {
            this.value = value;
            (rootName, path) = GetPathInHierarchy(value.transform);
        }

        public T Value => value != null ? value : (value = Load());

        T Load()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var root = rootObjects.FirstOrDefault(t => t.name == rootName);
            if (root == null)
            {
                return null;
            }

            var target = root.transform.Find(path);

            if (target == null)
            {
                return null;
            }

            var comp = target.GetComponent<T>();

            return comp;
        }

        static (string root, string relative) GetPathInHierarchy(Transform t)
        {
            var root = t.root;
            var rootPath = root.name;
            if (root == t)
            {
                return (rootPath, string.Empty);
            }

            var builder = new StringBuilder(t.name);
            var parent = t.parent;

            while (parent != root)
            {
                builder.Insert(0, parent.name + "/");
                parent = parent.parent;
            }

            return (rootPath, builder.ToString());
        }
    }
}
