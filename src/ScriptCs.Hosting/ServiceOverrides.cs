using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public abstract class ServiceOverrides<TConfig> : IServiceOverrides<TConfig>
        where TConfig : class, IServiceOverrides<TConfig>
    {
        public IDictionary<Type, object> Overrides { get; private set; }

        private readonly TConfig _this;

        protected ServiceOverrides()
        {
            _this = this as TConfig;
            Overrides = new Dictionary<Type, object>();
        }

        protected ServiceOverrides(IDictionary<Type, object> overrides)
            : this()
        {
            Overrides = overrides;
        }

        public TConfig ScriptHostFactory<T>() where T : IScriptHostFactory
        {
            Overrides[typeof(IScriptHostFactory)] = typeof(T);
            return _this;
        }

        public TConfig ScriptExecutor<T>() where T : IScriptExecutor
        {
            Overrides[typeof(IScriptExecutor)] = typeof(T);
            return _this;
        }

        public TConfig Repl<T>() where T : IRepl
        {
            Overrides[typeof(IRepl)] = typeof(T);
            return _this;
        }

        public TConfig ScriptEngine<T>() where T : IScriptEngine
        {
            Overrides[typeof(IScriptEngine)] = typeof(T);
            return _this;
        }

        public TConfig ScriptPackManager<T>() where T : IScriptPackManager
        {
            Overrides[typeof(IScriptPackManager)] = typeof(T);
            return _this;
        }

        public TConfig ScriptPackResolver<T>() where T : IScriptPackResolver
        {
            Overrides[typeof(IScriptPackResolver)] = typeof(T);
            return _this;
        }

        public TConfig InstallationProvider<T>() where T : IInstallationProvider
        {
            Overrides[typeof(IInstallationProvider)] = typeof(T);
            return _this;
        }

        public TConfig FileSystem<T>() where T : IFileSystem
        {
            Overrides[typeof(IFileSystem)] = typeof(T);
            return _this;
        }

        public TConfig AssemblyUtility<T>() where T : IAssemblyUtility
        {
            Overrides[typeof(IAssemblyUtility)] = typeof(T);
            return _this;
        }

        public TConfig ObjectSerializer<T>() where T : IObjectSerializer
        {
            Overrides[typeof(IObjectSerializer)] = typeof(T);
            return _this;
        }

        public TConfig PackageContainer<T>() where T : IPackageContainer
        {
            Overrides[typeof(IPackageContainer)] = typeof(T);
            return _this;
        }

        public TConfig PackageInstaller<T>() where T : IPackageInstaller
        {
            Overrides[typeof(IPackageInstaller)] = typeof(T);
            return _this;
        }

        public TConfig FilePreProcessor<T>() where T : IFilePreProcessor
        {
            Overrides[typeof(IFilePreProcessor)] = typeof(T);
            return _this;
        }

        public TConfig PackageAssemblyResolver<T>() where T : IPackageAssemblyResolver
        {
            Overrides[typeof(IPackageAssemblyResolver)] = typeof(T);
            return _this;
        }

        public TConfig AssemblyResolver<T>() where T : IAssemblyResolver
        {
            Overrides[typeof(IAssemblyResolver)] = typeof(T);
            return _this;
        }

        public TConfig Console<T>() where T : IConsole
        {
            Overrides[typeof(IConsole)] = typeof(T);
            return _this;
        }

        public TConfig LineProcessor<T>() where T : ILineProcessor
        {
            var key = typeof(ILineProcessor);
            object value;
            if (!Overrides.TryGetValue(key, out value) || value == null)
            {
                value = Enumerable.Empty<Type>();
            }

            IEnumerable<Type> processors;
            try
            {
                processors = (IEnumerable<Type>)value;
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidOperationException("The line processors override has an invalid type.", ex);
            }

            Overrides[typeof(ILineProcessor)] = new[] { typeof(T) }.Concat(processors).ToList();

            return _this;
        }
    }
}
