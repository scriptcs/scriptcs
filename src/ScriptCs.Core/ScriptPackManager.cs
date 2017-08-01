using System;
using System.Collections.Generic;
using ScriptCs.Contracts;
using ScriptCs.Exceptions;

namespace ScriptCs
{
    public class ScriptPackManager : IScriptPackManager
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
            var key = typeof(TContext);
            if (!_contexts.ContainsKey(key))
            {
                throw new ScriptPackException(string.Format("Tried to resolve a script pack '{0}', but such script pack is not available in the current execution context.", key));
            }

            return (TContext)_contexts[key];
        }
    }
}