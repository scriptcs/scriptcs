using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public abstract class ScriptPackContext : IScriptPackContext
    {
        public IRequirer Requirer { get; set; }

        public T Require<T>() where T:IScriptPackContext
        {
            return Requirer.Require<T>();
        }
    }
}
