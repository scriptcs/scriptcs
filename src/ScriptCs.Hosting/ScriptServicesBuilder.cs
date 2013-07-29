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
    public class ScriptServicesBuilder : ScriptServiceConfiguration<IScriptServicesBuilder>, IScriptServicesBuilder
    {
        private readonly IInitializationContainerFactory _initializationContainerFactory;
        private IRuntimeContainerFactory _runtimeContainerFactory;
        private readonly IConsole _console;
        private bool _debug = false;
        private bool _repl = false;
        private string _scriptName;
        private LogLevel _logLevel;
        private Type _scriptExecutorType;
        private Type _scriptEngineType;
        private ILog _logger;

        public ScriptServicesBuilder (IConsole console, ILog logger, IRuntimeContainerFactory runtimeContainerFactory = null)
        {
            _initializationContainerFactory = new InitializationContainerFactory(logger);
            _runtimeContainerFactory = runtimeContainerFactory;
            _console = console;
            _logger = logger;
        }

        public ScriptServices Build()
        {
            var defaultExecutorType = _debug ? typeof (DebugScriptExecutor) : typeof (ScriptExecutor);
            var defaultEngineType = _debug ? typeof (RoslynScriptDebuggerEngine) : typeof (RoslynScriptEngine);

            _scriptExecutorType = _overrides.ContainsKey(typeof(IScriptExecutor)) ? (Type)_overrides[typeof(IScriptExecutor)] : defaultExecutorType;
            _scriptEngineType = _overrides.ContainsKey(typeof(IScriptEngine)) ? (Type) _overrides[typeof(IScriptEngine)] : defaultEngineType;

            var initDirectoryCatalog = _scriptName != null || _repl;

            if (_runtimeContainerFactory == null)
            {
                _runtimeContainerFactory = new RuntimeContainerFactory(_logger, _overrides, _console,
                                                                       _scriptEngineType, _scriptExecutorType,
                                                                       initDirectoryCatalog,
                                                                       _initializationContainerFactory);
            }
            return _runtimeContainerFactory.GetScriptServices();
        }

        public void LoadModules(string extension, params string[] moduleNames)
        {
            var config = new ModuleConfiguration(_debug, _scriptName, _repl, _logLevel, _overrides);
            var loader = _initializationContainerFactory.GetModuleLoader();
            loader.Load(config, _initializationContainerFactory.GetFileSystem().ModulesFolder, extension, moduleNames);
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
