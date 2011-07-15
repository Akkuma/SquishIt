using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using SquishIt.Framework.Minifiers;

namespace SquishIt.Framework.Cachers
{
    public class CacherFactory
    {
        private static KeyedByTypeCollection<ICacher> Renderers = new KeyedByTypeCollection<ICacher>
        {
            new ApplicationCache()
        };

        public static T Get<T>() where T : ICacher
        {
            return Renderers.Find<T>();
        }
    }
}