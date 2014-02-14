using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackContextRegistry : IScriptPackContextRegistry
    {
        private IContainer _container;

        public void Initialze(IContainer container)
        {
            _container = container;
        }

        public void Register(Type type)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(type);
            builder.Update(_container);
        }
    }
}
