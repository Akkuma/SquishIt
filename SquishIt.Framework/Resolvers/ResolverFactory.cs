using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using SquishIt.Framework.Minifiers;

namespace SquishIt.Framework.Resolvers
{
    public class ResolverFactory
    {
        private static KeyedByTypeCollection<IResolver> resolvers = new KeyedByTypeCollection<IResolver>
        {
            new DirectoryResolver(),
            new EmbeddedResourceResolver(),
            new FileResolver(),
            new HttpResolver(),
        };

        public static T Get<T>() where T : IResolver
        {
            return resolvers.Find<T>();
        }
    }
}