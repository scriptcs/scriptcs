using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Roslyn.Scripting.CSharp;

namespace ScriptCs.Contracts
{
    [InheritedExport]
    public interface IScriptPack
    {
        IScriptPackContext Initialize(ISession session);
        void Terminate();
    }
}
