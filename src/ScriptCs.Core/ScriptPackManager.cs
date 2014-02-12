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
            :this(contexts, null)
        {
        }

        public ScriptPackManager(IEnumerable<IScriptPackContext> contexts, IDictionary<Type, IScriptPackContext> contextLookup)
        {
            Guard.AgainstNullArgument("contexts", contexts);
            
            if (contextLookup == null)
                contextLookup = new Dictionary<Type, IScriptPackContext>();

            _contexts = contextLookup;

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
            IScriptPackContext context;
            if (_contexts.TryGetValue(contextType, out context))
                return context;

            return null;
        }

        T IRequirer.Require<T>()
        {
            return Get<T>();
        }

        IScriptPackContext IRequirer.Require(Type context)
        {
            return Get(context);
        }
    }
}