using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Hosting;
using log4net.Core;

using LogLevel = ScriptCs.Contracts.LogLevel;

namespace ScriptCs
{
    public class ScriptServicesBuilder : ServiceOverrides<IScriptServicesBuilder>, IScriptServicesBuilder
    {
        private readonly IInitializationServices _initializationServices;
        private IRuntimeServices _runtimeServices;
        private readonly IConsole _console;
        private bool _repl = false;
        private bool _cache = false;
        private string _scriptName;
        private LogLevel _logLevel;
        private Type _scriptExecutorType;
        private Type _scriptEngineType;
        private ILog _logger;

        public ScriptServicesBuilder(IConsole console, ILog logger, IRuntimeServices runtimeServices = null)
        {
            _initializationServices = new InitializationServices(logger);
            _runtimeServices = runtimeServices;
            _console = console;
            _logger = logger;
        }

        public ScriptServices Build()
        {
            var defaultExecutorType = typeof(ScriptExecutor);
            Type defaultEngineType = _cache ? typeof(RoslynScriptPersistentEngine) : typeof(RoslynScriptInMemoryEngine);

            defaultEngineType = _repl ? typeof(RoslynScriptEngine) : defaultEngineType;

            _scriptExecutorType = Overrides.ContainsKey(typeof(IScriptExecutor)) ? (Type)Overrides[typeof(IScriptExecutor)] : defaultExecutorType;
            _scriptEngineType = Overrides.ContainsKey(typeof(IScriptEngine)) ? (Type)Overrides[typeof(IScriptEngine)] : defaultEngineType;

            var initDirectoryCatalog = _scriptName != null || _repl;

            if (_runtimeServices == null)
            {
                _runtimeServices = new RuntimeServices(_logger, Overrides, LineProcessors, _console,
                                                                       _scriptEngineType, _scriptExecutorType,
                                                                       initDirectoryCatalog,
                                                                       _initializationServices, _scriptName);
            }

            return _runtimeServices.GetScriptServices();
        }

        public IScriptServicesBuilder LoadModules(string extension, params string[] moduleNames)
        {
            var config = new ModuleConfiguration(_cache, _scriptName, _repl, _logLevel, Overrides);
            var loader = _initializationServices.GetModuleLoader();
            loader.Load(config, _initializationServices.GetFileSystem().ModulesFolder, extension, moduleNames);
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
    }
}