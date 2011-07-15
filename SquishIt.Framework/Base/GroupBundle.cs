using System.Collections.Generic;
using SquishIt.Framework.Minifiers;

namespace SquishIt.Framework.Base
{
    internal class GroupBundle
    {
        internal HashSet<Asset> Assets = new HashSet<Asset>();
        internal Dictionary<string, string> Attributes = new Dictionary<string, string>();
        internal int Order { get; set; }

        internal GroupBundle()
        { 
        }

        internal GroupBundle(Dictionary<string, string> attributes)
        {
            Attributes = attributes;
        }
    }
}