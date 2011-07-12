using System.Collections.Generic;
using SquishIt.Framework.Base;

namespace SquishIt.Framework.Cachers
{
	public interface ICacher
	{
        T Get<T>(string name) where T : BundleBase<T>;
		void Clear();
		bool ContainsKey(string key);
        bool TryGetValue<T>(string key, out T bundle) where T : BundleBase<T>;
        void Add<T>(string key, T currentBundle) where T : BundleBase<T>;
	}
}