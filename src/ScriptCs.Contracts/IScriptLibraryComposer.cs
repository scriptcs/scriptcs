using System.Collections.Generic;
using System.Text;

namespace ScriptCs.Contracts
{
    public interface IScriptLibraryComposer
    {
        void Compose(StringBuilder builder = null);
        string ScriptLibrariesFile { get; }
    }
}