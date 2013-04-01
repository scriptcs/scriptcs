﻿using System;
using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackManager : IScriptPackManager
    {
        private IDictionary<Type, IScriptPackContext> _contexts = new Dictionary<Type, IScriptPackContext>(); 

        public ScriptPackManager(IEnumerable<IScriptPackContext> contexts)
        {
            foreach (var context in contexts)
            {
                _contexts.Add(context.GetType(), context);
            }
        }

        public TContext Get<TContext>() where TContext : IScriptPackContext
        {
            return (TContext) _contexts[typeof (TContext)];
        }
    }
}