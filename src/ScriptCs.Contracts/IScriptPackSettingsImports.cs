using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IScriptPackSettingsImports
    {
        void Imports(params string[] imports);
    }
}
