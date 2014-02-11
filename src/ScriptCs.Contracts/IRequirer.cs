using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs.Contracts
{
    public interface IRequirer
    {
        T Require<T>() where T : IScriptPackContext;
    }
}


