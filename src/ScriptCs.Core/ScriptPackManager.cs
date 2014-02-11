using System;
using System.Collections.Generic;
using System.Net;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackManager : IScriptPackManager, IRequirer
    {
        private readonly IDictionary<Type, IScriptPackContext> _contexts = new Dictionary<Type, IScriptPackContext>(); 

        public ScriptPackManager(IEnumerable<IScriptPackContext> contexts)
        {
            Guard.AgainstNullArgument("contexts", contexts);

            foreach (var context in contexts)
            {
                _contexts[context.GetType()] = context;
            }
        }

        public TContext Get<TContext>() where TContext : IScriptPackContext
        {
            return (TContext)_contexts[typeof(TContext)];
        }

        public IScriptPackContext Get(Type contextType)
        {
            return _contexts[contextType];
        }

        T IRequirer.Require<T>()
        {
            return Get<T>();
        }
    }
}