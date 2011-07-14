using System.Collections.Generic;
using System.IO;

namespace SquishIt.Framework.Resolvers
{
    public class FileResolver: IResolver
    {
        public IEnumerable<string> Resolve(string file)
        {
            file = FileSystem.ResolveAppRelativePathToFileSystem(file);
            yield return Path.GetFullPath(file);            
        }        
    }
}