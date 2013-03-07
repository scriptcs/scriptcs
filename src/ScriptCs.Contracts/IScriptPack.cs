using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Scripting.CSharp;

namespace ScriptCs.Contracts
{
    public interface IScriptPack
    {
        void ConfigureSession(ISession session);
    }
}
