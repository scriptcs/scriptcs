using System;
using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting
{
    public abstract class ServiceOverrides<TConfig> : IServiceOverrides<TConfig>
        where TConfig : class, IServiceOverrides<TConfig>
    {
        private readonly IList<Type> lineProcessors = new List<Type>();
        private readonly IList<Type> codeRewriters = new List<Type>();
        private readonly TConfig _this;

        public readonly IDictionary<Type, object> Overrides = new Dictionary<Type, object>();

        protected ServiceOverrides()
        {
            _this = this as TConfig;
        }

        public IEnumerable<Type> LineProcessors
        {
            get
            {
                return lineProcessors;
            }
        }

        public IEnumerable<Type> CodeRewriters
        {
            get
            {
                return codeRewriters;
            }
        }

        public TConfig ScriptHostFactory<T>() where T : IScriptHostFactory
        {
            Overrides[typeof(IScriptHostFactory)] = typeof(T);
            return _this;
        }

        protected ServiceOverrides(IDictionary<Type, object> overrides)
        {
            Overrides = overrides;
        }

        public TConfig ScriptExecutor<T>() where T : IScriptExecutor
        {
            Overrides[typeof(IScriptExecutor)] = typeof(T);
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
            this.lineProcessors.Add(typeof(T));
            return _this;
        }

        public TConfig CodeRewriter<T>() where T : ICodeRewriter
        {
            this.codeRewriters.Add(typeof(T));
            return _this;
        }
    }
}