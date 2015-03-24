using System;
using System.Linq;
using ScriptCs.Contracts;
using ScriptCs.Logging;
using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs.Hosting
{
    public class ScriptServicesBuilder : ServiceOverrides<IScriptServicesBuilder>, IScriptServicesBuilder
    {
        private readonly ITypeResolver _typeResolver;
        private readonly ILog _logger;

        private IRuntimeServices _runtimeServices;
        private bool _repl;
        private bool _cache;
        private bool _debug;
        private string _scriptName;
        private LogLevel _logLevel;
        private Type _scriptExecutorType;
        private Type _replType;
        private Type _scriptEngineType;

        public ScriptServicesBuilder(
            IConsole console,
            ILog logger,
            IRuntimeServices runtimeServices = null,
            ITypeResolver typeResolver = null,
            IInitializationServices initializationServices = null)
        {
            InitializationServices = initializationServices ?? new InitializationServices(logger);
            _runtimeServices = runtimeServices;
            _typeResolver = typeResolver;
            _typeResolver = typeResolver ?? new TypeResolver();
            ConsoleInstance = console;
            _logger = logger;
        }

        public ScriptServices Build()
        {
            var defaultExecutorType = typeof(ScriptExecutor);
            _scriptExecutorType = Overrides.ContainsKey(typeof(IScriptExecutor))
                ? (Type)Overrides[typeof(IScriptExecutor)]
                : defaultExecutorType;

            var defaultReplType = typeof(Repl);
            _replType = Overrides.ContainsKey(typeof(IRepl)) ? (Type)Overrides[typeof(IRepl)] : defaultReplType;

            _scriptEngineType = (Type)Overrides[typeof(IScriptEngine)];

            var initDirectoryCatalog = _scriptName != null || _repl;

            if (_runtimeServices == null)
            {
                _runtimeServices = new RuntimeServices(
                    _logger,
                    Overrides,
                    ConsoleInstance,
                    _scriptEngineType,
                    _scriptExecutorType,
                    _replType,
                    initDirectoryCatalog,
                    InitializationServices,
                    _scriptName);
            }

            return _runtimeServices.GetScriptServices();
        }

        public IScriptServicesBuilder LoadModules(string extension, params string[] moduleNames)
        {
            var engineModule = _typeResolver.ResolveType("Mono.Runtime") != null || moduleNames.Contains("mono")
                ? "mono"
                : "roslyn";

            moduleNames = moduleNames.Union(new[] {engineModule}).ToArray();

            var config = new ModuleConfiguration(_cache, _scriptName, _repl, _logLevel, _debug, Overrides);
            var loader = InitializationServices.GetModuleLoader();

            var fs = InitializationServices.GetFileSystem();

            var folders = new[] {fs.GlobalFolder};
            loader.Load(config, folders, InitializationServices.GetFileSystem().HostBin, extension, moduleNames);
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