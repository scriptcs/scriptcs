using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IScriptCsModule
    {
        void Initialize(IScriptRuntimeBuilder builder);
    }
}
