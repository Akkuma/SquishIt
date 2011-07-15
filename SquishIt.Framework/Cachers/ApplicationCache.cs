using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using SquishIt.Framework.Base;

namespace SquishIt.Framework.Cachers
{
    public class ApplicationCache: ICacher
    {
        private const string KEY_PREFIX = "squishit_";

        private static HashSet<string> CacheKeys = new HashSet<string>();

        public T Get<T>(string name) where T : BundleBase<T>
        {
            return (T)HttpRuntime.Cache[KEY_PREFIX + name];
        }

        public void Clear()
        {
            foreach (var key in CacheKeys)
            {
                HttpRuntime.Cache.Remove(key);
            }

            CacheKeys.Clear();
        }

        public bool ContainsKey(string key)
        {
            return HttpRuntime.Cache[KEY_PREFIX + key] != null;
        }

        public bool TryGetValue<T>(string key, out T bundle) where T : BundleBase<T>
        {
            bundle = (T)HttpRuntime.Cache[KEY_PREFIX + key];

            return bundle != null;
        }

        public void Add<T>(string key, T currentBundle) where T : BundleBase<T>
        {
            key = KEY_PREFIX + key;
            CacheKeys.Add(key);
            HttpRuntime.Cache.Insert(key, currentBundle, new CacheDependency(currentBundle.DependentFiles.ToArray()),
                                    Cache.NoAbsoluteExpiration, 
                                    new TimeSpan(365, 0, 0, 0),
                                    CacheItemPriority.NotRemovable,
                                    Refresh<T>);
        }
        
        private void Refresh<T>(string key, object value, CacheItemRemovedReason removedReason) where T : BundleBase<T>
        {
            if (removedReason != CacheItemRemovedReason.Removed)
            {
                var bundle = (T)value;
                bundle.Render(bundle.RenderTo, bundle.Named, bundle.Renderer, bundle.Cacher);
            }
        }
    }
}