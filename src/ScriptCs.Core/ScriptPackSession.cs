using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptPackSession : IScriptPackSession
    {
        private readonly ISession _session;

        public ScriptPackSession(ISession session)
        {
            _session = session;
        }

        public void AddReference(string assemblyDisplayNameOrPath)
        {
            _session.AddReference(assemblyDisplayNameOrPath);
        }

        public void ImportNamespace(string ns)
        {
            
        }
    }
}
