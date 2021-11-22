using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanikama.Editor
{
    public class ObjectReference<T> where T : Component
    {
        T reference;
        readonly string rootName;
        readonly string path; // relative to root

        public ObjectReference(T value)
        {
            reference = value;
            (rootName, path) = GetPathInHierarchy(value.transform);
        }

        public T Ref => reference != null ? reference : (reference = Load());

        T Load()
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var root = rootObjects.FirstOrDefault(t => t.name == rootName);
            if (root == null)
            {
                // error
            }

            var target = root.transform.Find(path);

            if (target == null)
            {
                // error
            }

            var comp = target.GetComponent<T>();

            if (comp == null)
            {
                // error
            }

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
