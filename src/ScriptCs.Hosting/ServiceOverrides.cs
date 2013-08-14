using System;
using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public abstract class ServiceOverrides<TConfig> : IServiceOverrides<TConfig>
        where TConfig : class, IServiceOverrides<TConfig>
    {
        protected readonly IList<Type> LineProcessors = new List<Type>();

        protected readonly IDictionary<Type, object> Overrides = new Dictionary<Type, object>();

        private readonly TConfig _this;

        protected ServiceOverrides()
        {
            _this = this as TConfig;
        }

        protected ServiceOverrides(IDictionary<Type, object> overrides)
        {
            Overrides = overrides;
        }

        public TConfig WithScriptExecutor<T>() where T : IScriptExecutor
        {
            Overrides[typeof(IScriptExecutor)] = typeof(T);
            return _this;
        }

        public TConfig WithScriptEngine<T>() where T : IScriptEngine
        {
            Overrides[typeof(IScriptEngine)] = typeof(T);
            return _this;
        }

        public TConfig WithScriptHostFactory<T>() where T : IScriptHostFactory
        {
            Overrides[typeof(IScriptHostFactory)] = typeof(T);
            return _this;
        }

        public TConfig WithScriptPackManager<T>() where T : IScriptPackManager
        {
            Overrides[typeof(IScriptPackManager)] = typeof(T);
            return _this;
        }

        public TConfig WithScriptPackResolver<T>() where T : IScriptPackResolver
        {
            Overrides[typeof(IScriptPackResolver)] = typeof(T);
            return _this;
        }

        public TConfig WithInstallationProvider<T>() where T : IInstallationProvider
        {
            Overrides[typeof(IInstallationProvider)] = typeof(T);
            return _this;
        }

        public TConfig WithFileSystem<T>() where T : IFileSystem
        {
            Overrides[typeof(IFileSystem)] = typeof(T);
            return _this;
        }

        public TConfig WithAssemblyUtility<T>() where T : IAssemblyUtility
        {
            Overrides[typeof(IAssemblyUtility)] = typeof(T);
            return _this;
        }

        public TConfig WithPackageContainer<T>() where T : IPackageContainer
        {
            Overrides[typeof(IPackageContainer)] = typeof(T);
            return _this;
        }

        public TConfig WithPackageInstaller<T>() where T : IPackageInstaller
        {
            Overrides[typeof(IPackageInstaller)] = typeof(T);
            return _this;
        }

        public TConfig WithFilePreProcessor<T>() where T : IFilePreProcessor
        {
            Overrides[typeof(IFilePreProcessor)] = typeof(T);
            return _this;
        }

        public TConfig WithPackageAssemblyResolver<T>() where T : IPackageAssemblyResolver
        {
            Overrides[typeof(IPackageAssemblyResolver)] = typeof(T);
            return _this;
        }

        public TConfig WithAssemblyResolver<T>() where T : IAssemblyResolver
        {
            Overrides[typeof(IAssemblyResolver)] = typeof(T);
            return _this;
        }

        public TConfig WithLineProcessor<T>() where T : ILineProcessor
        {
            LineProcessors.Add(typeof(T));
            return _this;
        }
    }
}