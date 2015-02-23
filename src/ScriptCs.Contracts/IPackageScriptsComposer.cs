using System.Collections.Generic;
using System.Text;

namespace ScriptCs.Contracts
{
    public interface IPackageScriptsComposer
    {
        void Compose(StringBuilder builder = null);
        string PackageScriptsFile { get; }
    }
}