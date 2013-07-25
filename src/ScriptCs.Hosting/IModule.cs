using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public interface IModule
    {
        void Initialize(IScriptRuntimeBuilder builder, bool repl, bool debug);
    }
}
