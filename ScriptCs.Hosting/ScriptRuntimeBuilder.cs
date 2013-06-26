using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Engine.Roslyn;

namespace ScriptCs
{
    public class ScriptRuntimeBuilder
    {
        private bool _debug = false;
        private bool _repl = false;
        private string _scriptName;
        private LogLevel _logLevel;

        public ScriptRuntime Build()
        {
            Type scriptExecutorType;
            Type scriptEngineType;

            if (_debug)
            {
                scriptExecutorType = typeof(DebugScriptExecutor);
                scriptEngineType = typeof(RoslynScriptDebuggerEngine);
            }
            else
            {
                scriptExecutorType = typeof(ScriptExecutor);
                scriptEngineType = typeof(RoslynScriptEngine);
            }

            var loggerConfigurator = new LoggerConfigurator(_logLevel);

            var runtime = new ScriptRuntime(_scriptName, _repl, loggerConfigurator, new ScriptConsole(), scriptExecutorType, scriptEngineType);
            return runtime;
        }

        public ScriptRuntimeBuilder Debug(bool debug = true)
        {
            _debug = debug;
            return this;
        }

        public ScriptRuntimeBuilder ScriptName(string name)
        {
            _scriptName = name;
            return this;
        }

        public ScriptRuntimeBuilder Repl(bool repl = true)
        {
            _repl = repl;
            return this;
        }

        public ScriptRuntimeBuilder LogLevel(LogLevel level)
        {
            _logLevel = level;
            return this;
        }
    }
}
