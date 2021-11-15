using UnityEngine;

namespace Kanikama.Editor
{
    public static class KanikamaEditorUtil
    {
        public static string GetName(object obj)
        {
            return obj is Object ob ? ob.name : obj.GetType().Name;
        }
    }
}
