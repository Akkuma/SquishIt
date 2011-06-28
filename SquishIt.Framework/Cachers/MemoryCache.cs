using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace SquishIt.Framework.Cachers
{
    public class MemoryCache: ICacher
    {
        private static Dictionary<string, string> cache = new Dictionary<string,string>();

        public string Get(string name)
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

        public void Add(string key, string content, List<string> files)
        {
            cache.Add(key, content);
        }
    }
}