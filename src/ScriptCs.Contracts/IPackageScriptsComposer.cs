using System.Collections.Generic;
using System.Text;

namespace ScriptCs.Contracts
{
    public interface IPackageScriptsComposer
    {
        void Compose(IEnumerable<IPackageReference> packageReferences, StringBuilder builder = null);
    }
}