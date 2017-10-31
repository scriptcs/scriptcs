using ScriptCs.Contracts;
using System.Text;

namespace ScriptCs
{
    public class NullScriptLibraryComposer : IScriptLibraryComposer
    {
        public void Compose(string workingDirectory, StringBuilder builder = null)
        {
        }

        public string ScriptLibrariesFile => string.Empty;
    }
}
