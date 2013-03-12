using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class ScriptHost
    {
        public ScriptHost(IEnumerable<IScriptPackContext> contexts)
        {
            ScriptPackManager = new ScriptPackManager(contexts);
        }

        public ScriptPackManager ScriptPackManager { get; private set; }
    }
}
