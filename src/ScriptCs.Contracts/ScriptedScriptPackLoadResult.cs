using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public class ScriptedScriptPackLoadResult
    {
        public ScriptedScriptPackLoadResult()
        {
            
        }

        public ScriptedScriptPackLoadResult(IEnumerable<IScriptPack> scriptPacks, IEnumerable<Tuple<string, ScriptResult>> results)
        {
            ScriptPacks = scriptPacks;
            Results = results;
        }

        public virtual IEnumerable<IScriptPack> ScriptPacks { get; private set; }
        public virtual IEnumerable<Tuple<string, ScriptResult>> Results { get; private set; }
    }
}
