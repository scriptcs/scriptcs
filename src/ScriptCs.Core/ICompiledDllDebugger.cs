using System.ComponentModel.Composition;

namespace ScriptCs
{
    [InheritedExport]
    public interface ICompiledDllDebugger
    {
        void Run(string dllPath, ISession session);
    }
}
