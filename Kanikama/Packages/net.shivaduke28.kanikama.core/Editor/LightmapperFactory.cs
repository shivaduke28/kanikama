using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kanikama.Core.Editor
{
    public static class LightmapperFactory
    {
        struct IndexedFactory
        {
            public Func<ILightmapper> Factory { get; }
            public int SortingOrder { get; }

            public IndexedFactory(Func<ILightmapper> factory, int sortingOrder)
            {
                Factory = factory;
                SortingOrder = sortingOrder;
            }
        }

        static readonly Dictionary<string, IndexedFactory> map = new Dictionary<string, IndexedFactory>();

        public static void Register(string key, Func<ILightmapper> factory, int sortingOrder = 10)
        {
            Debug.Log("register:" + key);
            map[key] = new IndexedFactory(factory, sortingOrder);
        }

        public static string[] GetKeys() => map.OrderBy(kvp => kvp.Value.SortingOrder).Select(kvp => kvp.Key).ToArray();

        public static ILightmapper Create(string key)
        {
            if (map.TryGetValue(key, out var t))
            {
                return t.Factory.Invoke();
            }
            throw new ArgumentException($"Lightmapper with key {key} is not registered.");
        }
    }
}
