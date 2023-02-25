using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Core
{
    public class ComponentReference<T> where T : Component
    {
        T value;
        readonly string rootName;
        readonly string path; // relative to root

        public ComponentReference(T value)
        {
            this.value = value;
            (rootName, path) = GetPathInHierarchy(value.transform);
        }

        public T Value => value != null ? value : value = Load();

        T Load()
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var root = rootObjects.FirstOrDefault(t => t.name == rootName);
            if (root == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(path))
            {
                return root.GetComponent<T>();
            }

            var target = root.transform.Find(path);
            return target == null ? null : target.GetComponent<T>();
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
