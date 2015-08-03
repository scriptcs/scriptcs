using System;
using System.Linq;
using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public class ScriptServicesBuilder : ServiceOverrides<IScriptServicesBuilder>, IScriptServicesBuilder
    {
        private readonly ITypeResolver _typeResolver;
        private readonly ILogProvider _logProvider;

        private IRuntimeServices _runtimeServices;
        private bool _repl;
        private bool _cache;
        private bool _debug;
        private string _scriptName;
        private LogLevel _logLevel;
        private Type _scriptExecutorType;
        private Type _replType;
        private Type _scriptEngineType;
        private bool? _loadScriptPacks;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public ScriptServicesBuilder(
            IConsole console,
            Common.Logging.ILog logger,
            IRuntimeServices runtimeServices = null,
            ITypeResolver typeResolver = null,
            IInitializationServices initializationServices = null)
            : this(
                console,
                new CommonLoggingLogProvider(logger),
                runtimeServices,
                typeResolver,
                initializationServices)
        {
        }

        public ScriptServicesBuilder(
            IConsole console,
            ILogProvider logProvider,
            IRuntimeServices runtimeServices = null,
            ITypeResolver typeResolver = null,
            IInitializationServices initializationServices = null)
        {
            InitializationServices = initializationServices ?? new InitializationServices(logProvider);
            _runtimeServices = runtimeServices;
            _typeResolver = typeResolver;
            _typeResolver = typeResolver ?? new TypeResolver();
            ConsoleInstance = console;
            _logProvider = logProvider;
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

            bool initDirectoryCatalog;

            if (_loadScriptPacks.HasValue)
            {
                initDirectoryCatalog = _loadScriptPacks.Value;
            }
            else
            {
                initDirectoryCatalog = _scriptName != null || _repl;
            }

            if (_runtimeServices == null)
            {
                _runtimeServices = new RuntimeServices(
                    _logProvider,
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

            moduleNames = moduleNames.Union(new[] { engineModule }).ToArray();

            var config = new ModuleConfiguration(_cache, _scriptName, _repl, _logLevel, _debug, Overrides);
            var loader = InitializationServices.GetModuleLoader();

            var fs = InitializationServices.GetFileSystem();

            var folders = new[] { fs.GlobalFolder };
            loader.Load(config, folders, InitializationServices.GetFileSystem().HostBin, extension, moduleNames);
            return this;
        }

        public IScriptServicesBuilder LoadScriptPacks(bool load = true)
        {
            _loadScriptPacks = load;
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

        public IScriptServicesBuilder SetOverride<TContract, TImpl>(TImpl value) where TImpl : TContract
        {
            Overrides[typeof(TContract)] = value;
            return this;
        }

        public IScriptServicesBuilder SetOverride<TContract, TImpl>() where TImpl : TContract
        {
            Overrides[typeof(TContract)] = typeof(TImpl);
            return this;
        }

        public IInitializationServices InitializationServices { get; private set; }

        public IConsole ConsoleInstance { get; private set; }

        internal IRuntimeServices RuntimeServices
        {
            get { return _runtimeServices; }
        }
    }
}