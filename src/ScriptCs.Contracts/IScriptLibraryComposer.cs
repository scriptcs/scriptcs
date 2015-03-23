using System.Text;

namespace ScriptCs.Contracts
{
    public interface IScriptLibraryComposer
    {
        void Compose(string workingDirectory, StringBuilder builder = null);

        string ScriptLibrariesFile { get; }
    }
}
