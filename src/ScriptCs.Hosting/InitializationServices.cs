using System;
using System.Collections.Generic;
using Autofac;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;

namespace ScriptCs.Hosting
{
    public class InitializationServices : ScriptServicesRegistration, IInitializationServices
    {
        private readonly ILog _log;

        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public InitializationServices(Common.Logging.ILog logger, IDictionary<Type, object> overrides = null)
            : this(new CommonLoggingLogProvider(logger), overrides)
        {
        }

        public InitializationServices(ILogProvider logProvider, IDictionary<Type, object> overrides = null)
            : base(logProvider, overrides)
        {
            Guard.AgainstNullArgument("logProvider", logProvider);

            _log = logProvider.ForCurrentType();

        }

        protected override IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            _log.Debug("Registering initialization services");
            builder.RegisterInstance(this.LogProvider);
            builder.RegisterType<ScriptServicesBuilder>().As<IScriptServicesBuilder>();

            RegisterLineProcessors(builder);
            
            RegisterOverrideOrDefault<IFileSystem>(builder, b => b.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance());
            
            RegisterOverrideOrDefault<IAssemblyUtility>(builder, b => b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>().SingleInstance());
            
            RegisterOverrideOrDefault<IPackageContainer>(builder, b => b.RegisterType<PackageContainer>().As<IPackageContainer>().SingleInstance());
            
            RegisterOverrideOrDefault<IPackageAssemblyResolver>(builder, b => b.RegisterType<PackageAssemblyResolver>().As<IPackageAssemblyResolver>().SingleInstance());
            
            RegisterOverrideOrDefault<IAssemblyResolver>(builder, b => b.RegisterType<AssemblyResolver>().As<IAssemblyResolver>().SingleInstance());
            
            RegisterOverrideOrDefault<IInstallationProvider>(builder, b => b.RegisterType<NugetInstallationProvider>().As<IInstallationProvider>().SingleInstance());
            
            RegisterOverrideOrDefault<IPackageInstaller>(builder, b => b.RegisterType<PackageInstaller>().As<IPackageInstaller>().SingleInstance());
            
            RegisterOverrideOrDefault<IModuleLoader>(builder, b => b.RegisterType<ModuleLoader>().As<IModuleLoader>().SingleInstance());
            
            RegisterOverrideOrDefault<IAppDomainAssemblyResolver>(builder, b => b.RegisterType<AppDomainAssemblyResolver>().As<IAppDomainAssemblyResolver>().SingleInstance());

            RegisterOverrideOrDefault<IFilePreProcessor>(builder, b => b.RegisterType<FilePreProcessor>().As<IFilePreProcessor>().SingleInstance());

            return builder.Build();
        }

        private IAssemblyResolver _assemblyResolver;

        public IAssemblyResolver GetAssemblyResolver()
        {
            return GetService(ref _assemblyResolver);
        }

        private IModuleLoader _moduleLoader;

        public IModuleLoader GetModuleLoader()
        {
            return GetService(ref _moduleLoader);
        }

        private IFileSystem _fileSystem;

        public IFileSystem GetFileSystem()
        {
            return GetService(ref _fileSystem);
        }

        private IInstallationProvider _installationProvider;

        public IInstallationProvider GetInstallationProvider()
        {
            return GetService(ref _installationProvider);
        }

        private IPackageAssemblyResolver _packageAssemblyResolver;

        public IPackageAssemblyResolver GetPackageAssemblyResolver()
        {
            return GetService(ref _packageAssemblyResolver);
        }

        private IPackageInstaller _packageInstaller;

        public IPackageInstaller GetPackageInstaller()
        {
            return GetService(ref _packageInstaller);
        }

        private IAppDomainAssemblyResolver _appDomainAssemblyResolver;

        public IAppDomainAssemblyResolver GetAppDomainAssemblyResolver()
        {
            return GetService(ref _appDomainAssemblyResolver);
        }

        private IAssemblyUtility _assemblyUtility;

        public IAssemblyUtility GetAssemblyUtility()
        {
            return GetService(ref _assemblyUtility);
        }

        private T GetService<T>(ref T service )
        {
            if (Equals(service,null))
            {
                _log.Debug(string.Format("Resolving {0}", typeof(T).Name));
                service = Container.Resolve<T>();
            }

            return service;
        }
    }
}
