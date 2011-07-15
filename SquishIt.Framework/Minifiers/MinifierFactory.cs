using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using SquishIt.Framework.CSS;
using SquishIt.Framework.JavaScript;
using SquishIt.Framework.Minifiers;

namespace SquishIt.Framework.Minifiers
{
    public static class MinifierFactory
    {
        private static KeyedByTypeCollection<Framework.Minifiers.IMinify> Minifiers = new KeyedByTypeCollection<Framework.Minifiers.IMinify>
        {               
            new CSS.MsCompressor(),
            new CSS.NullCompressor(),
            new CSS.YuiCompressor(),
            new JavaScript.JsMinMinifier(),
            new JavaScript.NullMinifier(),
            new JavaScript.YuiMinifier(),
            new JavaScript.ClosureMinifier(),
            new JavaScript.MsMinifier()                
        };

        public static Min Get<Min>() where Min : IMinify
        {            
            return Minifiers.Find<Min>();
        }
    }
}