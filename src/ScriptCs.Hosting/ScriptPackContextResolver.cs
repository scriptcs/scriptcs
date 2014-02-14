using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackContextResolver : IScriptPackContextResolver
    {
        private IContainer _resolver;

        public void Initialize(IContainer container)
        {
            _resolver = container;
        }
  
        public IScriptPackContext Resolve(Type context)
        {
            return (IScriptPackContext) _resolver.Resolve(context);
        }
    }
}
