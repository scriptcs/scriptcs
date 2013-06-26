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
    public class ScriptRuntimeBuilder
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

            var runtime = new ScriptRuntime(_scriptName, _repl, loggerConfigurator, new ScriptConsole(), _scriptExecutorType, _scriptEngineType, _overrides);
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

        public ScriptRuntimeBuilder ScriptExecutor<T>() where T : IScriptExecutor
        {
            _scriptExecutorType = typeof (T);
            return this;
        }

        public ScriptRuntimeBuilder ScriptEngine<T>() where T : IScriptEngine
        {
            _scriptEngineType = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder ScriptHostFactory<T>() where T : IScriptHostFactory
        {
            _overrides[typeof(IScriptHostFactory)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder ScriptPackManager<T>() where T : IScriptPackManager
        {
            _overrides[typeof(IScriptPackManager)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder ScriptPackResolver<T>() where T : IScriptPackResolver
        {
            _overrides[typeof(IScriptPackResolver)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder InstallationProvider<T>() where T : IInstallationProvider
        {
            _overrides[typeof(IInstallationProvider)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder FileSystem<T>() where T : IFileSystem
        {
            _overrides[typeof(IFileSystem)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder AssemblyUtility<T>() where T : IAssemblyUtility
        {
            _overrides[typeof(IAssemblyUtility)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder PackageContainer<T>() where T : IPackageContainer
        {
            _overrides[typeof(IPackageContainer)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder FilePreProcessor<T>() where T : IFilePreProcessor
        {
            _overrides[typeof(IFilePreProcessor)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder PackageAssemblyResolver<T>() where T : IPackageAssemblyResolver
        {
            _overrides[typeof(IPackageAssemblyResolver)] = typeof(T);
            return this;
        }

        public ScriptRuntimeBuilder AssemblyResolver<T>() where T : IFilePreProcessor
        {
            _overrides[typeof(IAssemblyResolver)] = typeof(T);
            return this;
        }
  
    }
}
