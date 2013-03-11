using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    [InheritedExport]
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }
        IScriptHostFactory ScriptHostFactory { get; set; }

        void Execute(string code, IEnumerable<string> references, IEnumerable<IScriptPack> scriptPacks);
    }
}
