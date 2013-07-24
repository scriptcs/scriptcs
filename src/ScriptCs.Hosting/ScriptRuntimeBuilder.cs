using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public class ScriptRuntimeBuilder : IScriptRuntimeBuilder
    {
        private bool _debug = false;
        private bool _repl = false;
        private string _scriptName;
        private LogLevel _logLevel;
        private IDictionary<Type, object> _overrides = new Dictionary<Type, object>();
        private Type _scriptExecutorType;
        private Type _scriptEngineType;

        public ScriptRuntime Build()
        {
            if (_debug)
            {
                if (_scriptExecutorType == null) 
                    _scriptExecutorType = typeof(DebugScriptExecutor);

                if (_scriptEngineType == null)
                    _scriptEngineType = typeof(RoslynScriptDebuggerEngine);
            }
            else
            {
                if (_scriptExecutorType == null)
                    _scriptExecutorType = typeof(ScriptExecutor);
                
                if (_scriptEngineType == null)
                    _scriptEngineType = typeof(RoslynScriptEngine);
            }

            var loggerConfigurator = new LoggerConfigurator(_logLevel);
            var initDirectoryCatalog = _scriptName.Length > 0 || _repl;
            var factory = new ScriptContainerFactory(loggerConfigurator.GetLogger(), new ScriptConsole(), _scriptEngineType, _scriptExecutorType, initDirectoryCatalog, _overrides);
            var runtime = new ScriptRuntime(factory);
            return runtime;
        }

        public IScriptRuntimeBuilder Debug(bool debug = true)
        {
            _debug = debug;
            return this;
        }

        public IScriptRuntimeBuilder ScriptName(string name)
        {
            _scriptName = name;
            return this;
        }

        public IScriptRuntimeBuilder Repl(bool repl = true)
        {
            _repl = repl;
            return this;
        }

        public IScriptRuntimeBuilder LogLevel(LogLevel level)
        {
            _logLevel = level;
            return this;
        }

        public IScriptRuntimeBuilder ScriptExecutor<T>() where T : IScriptExecutor
        {
            _scriptExecutorType = typeof (T);
            return this;
        }

        public IScriptRuntimeBuilder ScriptEngine<T>() where T : IScriptEngine
        {
            _scriptEngineType = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder ScriptHostFactory<T>() where T : IScriptHostFactory
        {
            _overrides[typeof(IScriptHostFactory)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder ScriptPackManager<T>() where T : IScriptPackManager
        {
            _overrides[typeof(IScriptPackManager)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder ScriptPackResolver<T>() where T : IScriptPackResolver
        {
            _overrides[typeof(IScriptPackResolver)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder InstallationProvider<T>() where T : IInstallationProvider
        {
            _overrides[typeof(IInstallationProvider)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder FileSystem<T>() where T : IFileSystem
        {
            _overrides[typeof(IFileSystem)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder AssemblyUtility<T>() where T : IAssemblyUtility
        {
            _overrides[typeof(IAssemblyUtility)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder PackageContainer<T>() where T : IPackageContainer
        {
            _overrides[typeof(IPackageContainer)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder FilePreProcessor<T>() where T : IFilePreProcessor
        {
            _overrides[typeof(IFilePreProcessor)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder PackageAssemblyResolver<T>() where T : IPackageAssemblyResolver
        {
            _overrides[typeof(IPackageAssemblyResolver)] = typeof(T);
            return this;
        }

        public IScriptRuntimeBuilder AssemblyResolver<T>() where T : IFilePreProcessor
        {
            _overrides[typeof(IAssemblyResolver)] = typeof(T);
            return this;
        }
  
    }
}
