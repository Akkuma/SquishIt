using System.Collections.Generic;

namespace SquishIt.Framework.Cachers
{
	public interface ICacher
	{
		string Get(string name);
		void Clear();
		bool ContainsKey(string key);
		bool TryGetValue(string key, out string content);
		void Add(string key, string content, List<string> files);
	}
}