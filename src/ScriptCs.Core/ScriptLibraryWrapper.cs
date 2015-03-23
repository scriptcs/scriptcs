using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public abstract class ScriptLibraryWrapper 
    {
        private static IScriptHost _scriptHost;

        internal static IScriptHost ScriptHost
        {
            get
            {
                return _scriptHost;
            }
        }

        public static void SetHost(IScriptHost scriptHost)
        {
            _scriptHost = scriptHost;
        }

        public static T Require<T>() where T:IScriptPackContext
        {
            return _scriptHost.Require<T>();
        }

        public static IScriptEnvironment Env
        {
            get { return _scriptHost.Env; }
        }
    }
}
