namespace Scriptcs.Core
{
    using System.ComponentModel.Composition;

    using Roslyn.Scripting;

    [InheritedExport]
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }
        
        void AddReference(string assemblyDisplayNameOrPath);

        ISession CreateSession();
    }
}
