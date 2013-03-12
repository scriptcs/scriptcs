using System.ComponentModel.Composition;

namespace ScriptCs
{
    public interface ICompiledDllDebugger
    {
        void Run(string dllPath, ISession session);
    }
}
