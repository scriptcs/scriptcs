using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptInfo : IScriptInfo
    {
        public ScriptInfo()
        {
            LoadedScripts = new List<string>();
        }

        public string ScriptPath { get; set; }
        public IList<string> LoadedScripts { get; private set; }
    } 
}
