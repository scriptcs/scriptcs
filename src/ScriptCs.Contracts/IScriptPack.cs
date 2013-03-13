using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace ScriptCs.Contracts
{
    [InheritedExport]
    public interface IScriptPack
    {
        void Initialize(IScriptPackSession session);
        IScriptPackContext GetContext(); 
        void Terminate();
    }
}
