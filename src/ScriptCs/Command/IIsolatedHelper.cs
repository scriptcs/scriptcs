using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    public interface IIsolatedHelper
    {
        ScriptCsArgs CommandArgs { get; set; }
        string[] AssemblyPaths { get; set; }
        string Script { get; set; }
        string[] ScriptArgs { get; set; }
        ScriptResult Result { get; set; }

        void Execute();
    }
}