using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
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
