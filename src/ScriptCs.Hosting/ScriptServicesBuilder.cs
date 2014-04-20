using System;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;

using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Hosting
{
    public class ScriptServicesBuilder : ServiceOverrides<IScriptServicesBuilder>, IScriptServicesBuilder
    {
        private IRuntimeServices _runtimeServices;
        private bool _repl;
        private bool _cache;
        private bool _debug;
        private string _scriptName;
        private LogLevel _logLevel;
        private Type _scriptExecutorType;
        private Type _scriptEngineType;
        private ILog _logger;

        public ScriptServicesBuilder(IConsole console, ILog logger, IRuntimeServices runtimeServices = null)
        {
            InitializationServices = new InitializationServices(logger);
            _runtimeServices = runtimeServices;
            ConsoleInstance = console;
            _logger = logger;
        }

        public ScriptServices Build()
        {
            var defaultExecutorType = typeof(ScriptExecutor);
            var defaultEngineType = _cache ? typeof(RoslynScriptPersistentEngine) : typeof(RoslynScriptEngine);
            defaultEngineType = _debug ? typeof(RoslynScriptInMemoryEngine) : defaultEngineType;
            defaultEngineType = _repl ? typeof(RoslynScriptEngine) : defaultEngineType;

            _scriptExecutorType = Overrides.ContainsKey(typeof(IScriptExecutor)) ? (Type)Overrides[typeof(IScriptExecutor)] : defaultExecutorType;
            _scriptEngineType = Overrides.ContainsKey(typeof(IScriptEngine)) ? (Type)Overrides[typeof(IScriptEngine)] : defaultEngineType;

            var initDirectoryCatalog = _scriptName != null || _repl;

            if (_runtimeServices == null)
            {
                _runtimeServices = new RuntimeServices(_logger, Overrides, LineProcessors, ConsoleInstance,
                                                                       _scriptEngineType, _scriptExecutorType,
                                                                       initDirectoryCatalog,
                                                                       InitializationServices, _scriptName);
            }

            return _runtimeServices.GetScriptServices();
        }

        public IScriptServicesBuilder LoadModules(string extension, params string[] moduleNames)
        {
            var config = new ModuleConfiguration(_cache, _scriptName, _repl, _logLevel, Overrides);
            var loader = InitializationServices.GetModuleLoader();

            var fs = InitializationServices.GetFileSystem();
            var folders = _debug ? new[] { fs.ModulesFolder, fs.CurrentDirectory } : new[] { fs.ModulesFolder };
            loader.Load(config, folders, extension, moduleNames);
            return this;
        }

        public IScriptServicesBuilder Cache(bool cache = true)
        {
            _cache = cache;
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

        public IScriptServicesBuilder Debug(bool debug = true)
        {
            _debug = debug;
            return this;
        }

        public IInitializationServices InitializationServices { get; private set; }

        public IConsole ConsoleInstance { get; private set; }
    }
}