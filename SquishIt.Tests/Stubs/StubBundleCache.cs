using System.Collections.Generic;
using SquishIt.Framework;
using SquishIt.Framework.Base;
using SquishIt.Framework.Cachers;

namespace SquishIt.Tests.Stubs
{
	public class StubBundleCache: ICacher
	{
		private Dictionary<string, object> cache = new Dictionary<string, object>();

		public T Get<T>(string name) where T : BundleBase<T>
		{
			return (T)cache[name];
		}

		public void Clear()
		{
			cache = new Dictionary<string, object>();
		}

		public bool ContainsKey(string key)
		{
			return cache.ContainsKey(key);
		}

		public bool TryGetValue<T>(string key, out T bundle) where T : BundleBase<T>
		{
            object obj = null;
			var succeeded = cache.TryGetValue(key, out obj);
            bundle = (T)obj;
            return succeeded;
		}

        public void Add<T>(string key, T currentBundle) where T : BundleBase<T>
		{
            if (currentBundle.Named != null)
				cache.Add(key, currentBundle.Tag);
		}
	}
}