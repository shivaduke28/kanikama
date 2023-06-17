using Kanikama.Utility;
using UnityEditor;

namespace Kanikama.Editor.Baking.GUI
{
    [CustomEditor(typeof(RendererMaterialHolder))]
    public class RendererMaterialHolderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            if (KanikamaGUI.Button("Clear"))
            {
                var holder = (RendererMaterialHolder) target;
                holder.Clear();
            }
        }
    }
}
