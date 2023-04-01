using UnityEditor;

namespace Kanikama.Core.Editor
{
    public sealed class ObjectHandle<T> where T : UnityEngine.Object

    {
        readonly GlobalObjectId globalObjectId;
        T value;

        public T Value => value != null ? value : value = FindSlow();

        T FindSlow() => (T) GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);

        public ObjectHandle(GlobalObjectId globalObjectId)
        {
            this.globalObjectId = globalObjectId;
        }

        public override string ToString() => globalObjectId.ToString();
        public string LocalFileId() => globalObjectId.targetObjectId.ToString();
    }
}
