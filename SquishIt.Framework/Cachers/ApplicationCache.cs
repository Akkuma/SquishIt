using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace SquishIt.Framework.Cachers
{
    public class ApplicationCache: ICacher
    {
        private const string KEY_PREFIX = "squishit_";

        private static List<string> CacheKeys = new List<string>();

        public string Get(string name)
        {
            return (string)HttpRuntime.Cache[KEY_PREFIX + name];
        }

        public void Clear()
        {
            foreach (var key in CacheKeys)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }

        public bool ContainsKey(string key)
        {
            return HttpRuntime.Cache[KEY_PREFIX + key] != null;
        }

        public bool TryGetValue(string key, out string content)
        {
            content = (string)HttpRuntime.Cache[KEY_PREFIX + key];

            return content != null;
        }

        public void Add(string key, string content, List<string> files)
        {
            CacheKeys.Add(KEY_PREFIX + key);
            HttpRuntime.Cache.Add(KEY_PREFIX + key, content, new CacheDependency(files.ToArray()),
                                            Cache.NoAbsoluteExpiration, 
                                            new TimeSpan(365, 0, 0, 0),
                                            CacheItemPriority.NotRemovable,
                                            null);
        }
    }
}