using System;
using System.Collections.Generic;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace ScriptCs
{
    public abstract class ScriptServiceConfiguration<TConfig> : IScriptServiceConfiguration<TConfig> where TConfig:class,IScriptServiceConfiguration<TConfig>
    {
        protected readonly IDictionary<Type, object> _overrides = new Dictionary<Type, object>();
        private readonly TConfig _this;

        public ScriptServiceConfiguration()
        {
            _this = this as TConfig;
        }

        public ScriptServiceConfiguration(IDictionary<Type, object> overrides)
        {
            _overrides = overrides;
        } 

        public TConfig ScriptExecutor<T>() where T : IScriptExecutor
        {
            _overrides[typeof(IScriptExecutor)] = typeof(T);
            return _this;
        }

        public TConfig ScriptEngine<T>() where T : IScriptEngine
        {
            _overrides[typeof(IScriptEngine)] = typeof(T);
            return _this;
        }

        public TConfig ScriptHostFactory<T>() where T : IScriptHostFactory
        {
            _overrides[typeof(IScriptHostFactory)] = typeof(T);
            return _this;
        }

        public TConfig ScriptPackManager<T>() where T : IScriptPackManager
        {
            _overrides[typeof(IScriptPackManager)] = typeof(T);
            return _this;
        }

        public TConfig ScriptPackResolver<T>() where T : IScriptPackResolver
        {
            _overrides[typeof(IScriptPackResolver)] = typeof(T);
            return _this;
        }

        public TConfig InstallationProvider<T>() where T : IInstallationProvider
        {
            _overrides[typeof(IInstallationProvider)] = typeof(T);
            return _this;
        }

        public TConfig FileSystem<T>() where T : IFileSystem
        {
            _overrides[typeof(IFileSystem)] = typeof(T);
            return _this;
        }

        public TConfig AssemblyUtility<T>() where T : IAssemblyUtility
        {
            _overrides[typeof(IAssemblyUtility)] = typeof(T);
            return _this;
        }

        public TConfig PackageContainer<T>() where T : IPackageContainer
        {
            _overrides[typeof(IPackageContainer)] = typeof(T);
            return _this;
        }

        public TConfig FilePreProcessor<T>() where T : IFilePreProcessor
        {
            _overrides[typeof(IFilePreProcessor)] = typeof(T);
            return _this;
        }

        public TConfig PackageAssemblyResolver<T>() where T : IPackageAssemblyResolver
        {
            _overrides[typeof(IPackageAssemblyResolver)] = typeof(T);
            return _this;
        }

        public TConfig AssemblyResolver<T>() where T : IFilePreProcessor
        {
            _overrides[typeof(IAssemblyResolver)] = typeof(T);
            return _this;
        }
    }
}