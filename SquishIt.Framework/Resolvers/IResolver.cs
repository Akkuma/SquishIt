using System.Collections.Generic;

namespace SquishIt.Framework.Resolvers
{
    public interface IResolver
    {        
        IEnumerable<string> Resolve(string file);
    }
}