using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IScriptPackSettings : IScriptPackSettingsReferences, IScriptPackSettingsImports
    {
        IEnumerable<string> GetReferences();
        IEnumerable<string> GetImports();
        Type GetContextType();
    }
}
