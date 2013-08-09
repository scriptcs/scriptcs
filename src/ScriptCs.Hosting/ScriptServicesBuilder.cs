using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Hosting;
using log4net.Core;

namespace ScriptCs
{
    public class ScriptServicesBuilder : ServiceOverrides<IScriptServicesBuilder>, IScriptServicesBuilder
    {
        private readonly IInitializationServices _initializationServices;
        private IRuntimeServices _runtimeServices;
        private readonly IConsole _console;
        private bool _debug = false;
        private bool _repl = false;
        private string _scriptName;
        private LogLevel _logLevel;
        private Type _scriptExecutorType;
        private Type _scriptEngineType;
        private ILog _logger;

        public ScriptServicesBuilder (IConsole console, ILog logger, IRuntimeServices runtimeServices = null)
        {
            _initializationServices = new InitializationServices(logger);
            _runtimeServices = runtimeServices;
            _console = console;
            _logger = logger;
        }

        public ScriptServices Build()
        {
            var defaultExecutorType = _debug ? typeof (DebugScriptExecutor) : typeof (ScriptExecutor);
            var defaultEngineType = _debug ? typeof (RoslynScriptDebuggerEngine) : typeof (RoslynScriptEngine);

            _scriptExecutorType = Overrides.ContainsKey(typeof(IScriptExecutor)) ? (Type)Overrides[typeof(IScriptExecutor)] : defaultExecutorType;
            _scriptEngineType = Overrides.ContainsKey(typeof(IScriptEngine)) ? (Type) Overrides[typeof(IScriptEngine)] : defaultEngineType;

            var initDirectoryCatalog = _scriptName != null || _repl;

            if (_runtimeServices == null)
            {
                _runtimeServices = new RuntimeServices(_logger, Overrides, LineProcessors, _console,
                                                                       _scriptEngineType, _scriptExecutorType,
                                                                       initDirectoryCatalog,
                                                                       _initializationServices);
            }
            return _runtimeServices.GetScriptServices();
        }

        public IScriptServicesBuilder LoadModules(string extension, params string[] moduleNames)
        {
            var config = new ModuleConfiguration(_debug, _scriptName, _repl, _logLevel, Overrides);
            var loader = _initializationServices.GetModuleLoader();
            loader.Load(config, _initializationServices.GetFileSystem().ModulesFolder, extension, moduleNames);
            return this;
        }

        public IScriptServicesBuilder Debug(bool debug = true)
        {
            _debug = debug;
            return this;
        }

        public IScriptServicesBuilder ScriptName(string name)
        {
            _scriptName = name;
            return this;
        }

        public IScriptServicesBuilder Repl(bool repl = true)
        {
            _repl = repl;
            return this;
        }

        public IScriptServicesBuilder LogLevel(LogLevel level)
        {
            _logLevel = level;
            return this;
        }
    }
}
