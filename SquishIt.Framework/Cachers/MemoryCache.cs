using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using SquishIt.Framework.Base;

namespace SquishIt.Framework.Cachers
{/*
    public class MemoryCache: ICacher
    {
        private static Dictionary<string, string> cache = new Dictionary<string,string>();

        public string Get<T>(string name) where T : BundleBase<T>
        {
            return cache[name];
        }

        public void Clear()
        {
            cache.Clear();
        }

        public bool ContainsKey(string key)
        {
            return cache.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string content)
        {
            return cache.TryGetValue(key, out content);
        }

        public void Add<T>(string key, T currentBundle) where T : BundleBase<T>
        {
            cache.Add(key, currentBundle);
        }
    }
  */
}