using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Logging;
using NuGet;
using ScriptCs.Contracts;

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
            Type defaultExecutorType = typeof (ScriptExecutor);
            _scriptExecutorType = Overrides.ContainsKey(typeof(IScriptExecutor)) ? (Type)Overrides[typeof(IScriptExecutor)] : defaultExecutorType;
            _scriptEngineType = (Type)Overrides[typeof(IScriptEngine)];

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

        private string GetEngineModule(string[] modules)
        {
            if (Type.GetType("Mono.Runtime") == null && modules.Contains("mono"))
            {
                return "mono";
            }
            else
            {
                return "roslyn";
            }
        }

        public IScriptServicesBuilder LoadModules(string extension, params string[] moduleNames)
        {
            moduleNames = moduleNames.Union(new[] {GetEngineModule(moduleNames)}).ToArray();
            var config = new ModuleConfiguration(_cache, _scriptName, _repl, _logLevel, _debug, Overrides);
            var loader = InitializationServices.GetModuleLoader();

            var fs = InitializationServices.GetFileSystem();

            var folders = _debug ? new[] { fs.ModulesFolder, AppDomain.CurrentDomain.BaseDirectory, fs.CurrentDirectory } : new[] { fs.ModulesFolder, AppDomain.CurrentDomain.BaseDirectory };
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