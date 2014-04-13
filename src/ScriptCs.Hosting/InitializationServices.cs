using System;
using System.Collections.Generic;
using Autofac;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;

namespace ScriptCs.Hosting
{
    public class InitializationServices : ScriptServicesRegistration, IInitializationServices
    {
        public InitializationServices(ILog logger, IDictionary<Type, object> overrides = null)
            : base(logger, overrides)
        {
        }

        protected override IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            this.Logger.Debug("Registering initialization services");
            builder.RegisterInstance<ILog>(this.Logger);
            builder.RegisterType<ScriptServicesBuilder>().As<IScriptServicesBuilder>();
            RegisterOverrideOrDefault<IFileSystem>(builder, b => b.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyUtility>(builder, b => b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>().SingleInstance());
            RegisterOverrideOrDefault<IPackageContainer>(builder, b => b.RegisterType<PackageContainer>().As<IPackageContainer>().SingleInstance());
            RegisterOverrideOrDefault<IPackageAssemblyResolver>(builder, b => b.RegisterType<PackageAssemblyResolver>().As<IPackageAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyResolver>(builder, b => b.RegisterType<AssemblyResolver>().As<IAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IInstallationProvider>(builder, b => b.RegisterType<NugetInstallationProvider>().As<IInstallationProvider>().SingleInstance());
            RegisterOverrideOrDefault<IPackageInstaller>(builder, b => b.RegisterType<PackageInstaller>().As<IPackageInstaller>().SingleInstance());
            RegisterOverrideOrDefault<IModuleLoader>(builder, b => b.RegisterType<ModuleLoader>().As<IModuleLoader>().SingleInstance());
            return builder.Build();
        }

        private IAssemblyResolver _assemblyResolver;

        public IAssemblyResolver GetAssemblyResolver()
        {
            if (_assemblyResolver == null)
            {
                this.Logger.Debug("Resolving AssemblyResolver");
                _assemblyResolver = Container.Resolve<IAssemblyResolver>();
            }

            return _assemblyResolver;
        }

        private IModuleLoader _moduleLoader;

        public IModuleLoader GetModuleLoader()
        {
            if (_moduleLoader == null)
            {
                this.Logger.Debug("Resolving ModuleLoader");
                _moduleLoader = Container.Resolve<IModuleLoader>();
            }

            return _moduleLoader;
        }

        private IFileSystem _fileSystem;

        public IFileSystem GetFileSystem()
        {
            if (_fileSystem == null)
            {
                this.Logger.Debug("Resolving FileSystem");
                _fileSystem = Container.Resolve<IFileSystem>();
            }

            return _fileSystem;
        }

        private IInstallationProvider _installationProvider;

        public IInstallationProvider GetInstallationProvider()
        {
            if (_installationProvider == null)
            {
                this.Logger.Debug("Resolving Installation Provider");
                _installationProvider = Container.Resolve<IInstallationProvider>();
            }

            return _installationProvider;
        }

        private IPackageAssemblyResolver _packageAssemblyResolver;

        public IPackageAssemblyResolver GetPackageAssemblyResolver()
        {
            if (_packageAssemblyResolver == null)
            {
                this.Logger.Debug("Resolving Package Assembly Resolver");
                _packageAssemblyResolver = Container.Resolve<IPackageAssemblyResolver>();
            }

            return _packageAssemblyResolver;
        }

        private IPackageInstaller _packageInstaller;

        public IPackageInstaller GetPackageInstaller()
        {
            if (_packageInstaller == null)
            {
                this.Logger.Debug("Resolving Package Installer");
                _packageInstaller = Container.Resolve<IPackageInstaller>();
            }

            return _packageInstaller;
        }
    }
}