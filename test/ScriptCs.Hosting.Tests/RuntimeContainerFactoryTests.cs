using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class RuntimeContainerFactoryTests
    {
        public class TheCreateInitializationContainerMethod
        {

        }

        public class TheCreateRuntimeContainerMethod
        {
            private Mock<IConsole> _mockConsole = new Mock<IConsole>();
            private Type _scriptExecutorType = null;
            private Type _scriptEngineType = null;
            private Mock<ILog> _mockLogger = new Mock<ILog>();
            private IDictionary<Type, object> _overrides = new Dictionary<Type, object>();
            private RuntimeServices _runtimeServices = null;

            public TheCreateRuntimeContainerMethod()
            {
                var mockScriptExecutorType = new Mock<IScriptExecutor>();
                _scriptExecutorType = mockScriptExecutorType.Object.GetType();

                var mockScriptEngineType = new Mock<IScriptEngine>();
                _scriptEngineType = mockScriptEngineType.Object.GetType();

                var initializationContainerFactory = new InitializationServices(_mockLogger.Object, _overrides);
                _runtimeServices = new RuntimeServices(_mockLogger.Object, _overrides, _mockConsole.Object, _scriptEngineType, _scriptExecutorType, false, initializationContainerFactory);
            }

            [Fact]
            public void ShouldRegisterTheLoggerInstance()
            {
                var logger = _runtimeServices.Container.Resolve<ILog>();
                logger.ShouldEqual(_mockLogger.Object);
            }

            [Fact]
            public void ShouldRegisterTheScriptEngine()
            {
                var engine = _runtimeServices.Container.Resolve<IScriptEngine>();
                engine.GetType().ShouldEqual(_scriptEngineType);
            }

            [Fact]
            public void ShouldRegisterTheExecutor()
            {
                var executor = _runtimeServices.Container.Resolve<IScriptExecutor>();
                executor.GetType().ShouldEqual(_scriptExecutorType);
            }

            [Fact]
            public void ShouldRegisterTheConsoleInstance()
            {
                _runtimeServices.Container.Resolve<IConsole>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheScriptServices()
            {
                _runtimeServices.Container.Resolve<ScriptServices>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultScriptHostFactoryIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IScriptHostFactory>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultFilePreProcessorIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IFilePreProcessor>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultScriptPackResolverIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IScriptPackResolver>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultInstallationProviderIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IInstallationProvider>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageInstallerIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IPackageInstaller>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultScriptServiceRootIfNoOverride()
            {
                _runtimeServices.Container.Resolve<ScriptServices>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultFileSystemIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IFileSystem>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultAssemblyUtilityIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IAssemblyUtility>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageContainerIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IPackageContainer>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageAssemblyResolverIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IPackageAssemblyResolver>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultAssemblyResolverIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IAssemblyResolver>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheOverriddenScriptHostFactory()
            {
                var mock = new Mock<IScriptHostFactory>();
                _overrides[typeof(IScriptHostFactory)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IScriptHostFactory>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenFilePreProcessor()
            {
                var mock = new Mock<IFilePreProcessor>();
                _overrides[typeof(IFilePreProcessor)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IFilePreProcessor>().ShouldBeType(mock.Object.GetType());

            }

            [Fact]
            public void ShouldRegisterTheOverriddenScriptPackResolver()
            {
                var mock = new Mock<IScriptPackResolver>();
                _overrides[typeof(IScriptPackResolver)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IScriptPackResolver>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenInstallationProvider()
            {
                var mock = new Mock<IInstallationProvider>();
                _overrides[typeof(IInstallationProvider)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IInstallationProvider>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageInstaller()
            {
                var mock = new Mock<IPackageInstaller>();
                _overrides[typeof(IPackageInstaller)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IPackageInstaller>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenFileSystem()
            {
                var mock = new Mock<IFileSystem>();
                _overrides[typeof(IFileSystem)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IFileSystem>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyUtility()
            {
                var mock = new Mock<IAssemblyUtility>();
                _overrides[typeof(IAssemblyUtility)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IAssemblyUtility>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageContainer()
            {
                var mock = new Mock<IPackageContainer>();
                _overrides[typeof(IPackageContainer)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IPackageContainer>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageAssemblyResolver()
            {
                var mock = new Mock<IPackageAssemblyResolver>();
                _overrides[typeof(IPackageAssemblyResolver)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IPackageAssemblyResolver>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyResolver()
            {
                var mock = new Mock<IAssemblyResolver>();
                _overrides[typeof(IAssemblyResolver)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IAssemblyResolver>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyResolverInstance()
            {
                var mock = new Mock<IAssemblyResolver>();
                _overrides[typeof(IAssemblyResolver)] = mock.Object;
                _runtimeServices.Container.Resolve<IAssemblyResolver>().ShouldEqual(mock.Object);
            }
        }
    }
}
