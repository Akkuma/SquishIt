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
        private static Dictionary<string, ICacher> Renderers = new Dictionary<string, ICacher>
        {
            {typeof(ApplicationCache).FullName, new ApplicationCache()},
            {typeof(MemoryCache).FullName, new MemoryCache()}
        };

        public static T Get<T>() where T : ICacher
        {
            return (T)Renderers[typeof(T).FullName];
        }
    }
}