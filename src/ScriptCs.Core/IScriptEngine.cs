using System;
using System.ComponentModel.Composition;
using Roslyn.Scripting;

namespace ScriptCs
{
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }
        
        void AddReference(string assemblyDisplayNameOrPath);

        ISession CreateSession<THostObject>(THostObject hostObject) where THostObject : class;
        
        ISession CreateSession(object hostObject, Type hostObjectType = null);

        ISession CreateSession();
    }
}
