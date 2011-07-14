using System.Collections.Generic;
using SquishIt.Framework.Directories;

namespace SquishIt.Framework.Resolvers
{
    public class DirectoryResolver: IResolver
    {        
        private readonly IDirectoryEnumerator directoryEnumerator;

        public DirectoryResolver()
        {
            this.directoryEnumerator = new DirectoryEnumerator();
        }

        public DirectoryResolver(IDirectoryEnumerator directoryEnumerator)
        {
            this.directoryEnumerator = directoryEnumerator;
        }        

        public IEnumerable<string> Resolve(string directory)
        {
            return directoryEnumerator.GetFiles(directory);            
        }        
    }
}