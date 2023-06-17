using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.Editor.Baking
{
    [Serializable]
    public sealed class Lightmap
    {
        [SerializeField] public string Type;
        [SerializeField] public Texture2D Texture;
        [SerializeField] public string Path;
        [SerializeField] public int Index;

        public Lightmap(string type, Texture2D texture, string path, int index)
        {
            Type = type;
            Texture = texture;
            Path = path;
            Index = index;
        }
    }

    [Serializable]
    public sealed class LightmapArray
    {
        [SerializeField] public string Type;
        [SerializeField] public Texture2DArray Texture;
        [SerializeField] public string Path;
        [SerializeField] public int Index;

        public LightmapArray(string type, Texture2DArray texture, string path, int index)
        {
            Type = type;
            Texture = texture;
            Path = path;
            Index = index;
        }
    }

    [Serializable]
    public class KeyValueListPair<T>
    {
        public string Name;
        public string Key;
        public List<T> Values;
    }

    [Serializable]
    public sealed class KeyLightmapListPair : KeyValueListPair<Lightmap>
    {
    }

    [Serializable]
    public sealed class KeyLightmapArrayListPair : KeyValueListPair<LightmapArray>
    {
    }

    [Serializable]
    public sealed class LightmapStorage : KeyValueListMap<Lightmap, KeyLightmapListPair>
    {
    }

    [Serializable]
    public sealed class LightmapArrayStorage : KeyValueListMap<LightmapArray, KeyLightmapArrayListPair>
    {
    }

    [Serializable]
    public class AssetStorage
    {
        [SerializeField] LightmapStorage lightmapStorage;
        [SerializeField] LightmapArrayStorage lightmapArrayStorage;
        public LightmapStorage LightmapStorage => lightmapStorage;
        public LightmapArrayStorage LightmapArrayStorage => lightmapArrayStorage;
    }


    [Serializable]
    public class KeyValueListMap<T, TR> where TR : KeyValueListPair<T>, new()
    {
        [SerializeField] List<TR> valueListPairs = new List<TR>();
        public List<T> GetAll() => valueListPairs.SelectMany(x => x.Values).ToList();

        public void AddOrUpdate(string key, List<T> values, string name = "")
        {
            foreach (var pair in valueListPairs)
            {
                if (pair.Key == key)
                {
                    pair.Values = values;
                    pair.Name = name;
                    return;
                }
            }

            valueListPairs.Add(new TR
            {
                Name = name, Key = key, Values = values,
            });
        }

        public bool Remove(string key)
        {
            var index = valueListPairs.FindIndex(pair => pair.Key == key);
            if (index >= 0)
            {
                valueListPairs.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool TryGet(string key, out List<T> data)
        {
            foreach (var pair in valueListPairs)
            {
                if (pair.Key == key)
                {
                    data = pair.Values;
                    return true;
                }
            }

            data = default;
            return false;
        }

        public void Clear()
        {
            valueListPairs.Clear();
        }
    }
}
