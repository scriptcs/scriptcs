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
using Xunit;
using Should;

namespace ScriptCs.Tests
{
    public class ScriptRuntimeTests
    {
        public class TheInitializeMethod
        {
            private Mock<IConsole> _mockConsole = new Mock<IConsole>();
            private Type _scriptExecutorType = null;
            private Type _scriptEngineType = null;
            private Mock<ILoggerConfigurator> _mockLoggerConfigurator = new Mock<ILoggerConfigurator>();
            private Mock<ILog> _mockLogger = new Mock<ILog>();
            private IDictionary<Type, object> _overrides = new Dictionary<Type, object>(); 

            public TheInitializeMethod()
            {
                var mockScriptExecutorType = new Mock<IScriptExecutor>();
                _scriptExecutorType = mockScriptExecutorType.Object.GetType();

                var mockScriptEngineType = new Mock<IScriptEngine>();
                _scriptEngineType = mockScriptEngineType.Object.GetType();

                _mockLoggerConfigurator.Setup(l => l.GetLogger()).Returns(_mockLogger.Object);
            }

            [Fact]
            public void ShouldInvokeTheConfigureMethod() 
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                runtime.Initialize();
                _mockLoggerConfigurator.Verify(l=>l.Configure(It.IsAny<IConsole>()));
                
            }

            [Fact]
            public void ShouldGetTheLogger()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                runtime.Initialize();
                _mockLoggerConfigurator.Verify(l=>l.GetLogger());
            }

            [Fact]
            public void ShouldRegisterTheLoggerInstance()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                var logger = container.Resolve<ILog>();
                logger.ShouldEqual(_mockLogger.Object);
            }

            [Fact]
            public void ShouldRegisterTheScriptEngine()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                var engine = container.Resolve<IScriptEngine>();
                engine.GetType().ShouldEqual(_scriptEngineType);
            }

            [Fact]
            public void ShouldRegisterTheExecutor()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                var executor = container.Resolve<IScriptExecutor>();
                executor.GetType().ShouldEqual(_scriptExecutorType);
            }

            [Fact]
            public void ShouldRegisterTheConsoleInstance()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IConsole>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheScriptServices()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<ScriptServices>().ShouldNotBeNull();
            }


            [Fact]
            public void ShouldRegisterTheDefaultScriptHostFactoryIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IScriptHostFactory>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultFilePreProcessorIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IFilePreProcessor>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultScriptPackResolverIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IScriptPackResolver>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultInstallationProviderIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IInstallationProvider>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageInstallerIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IPackageInstaller>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultScriptServiceRootIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<ScriptServices>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultFileSystemIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IFileSystem>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultAssemblyUtilityIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IAssemblyUtility>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageContainerIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IPackageContainer>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageAssemblyResolverIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                var container = runtime.Initialize();
                container.Resolve<IPackageAssemblyResolver>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultAssemblyResolverIfNoOverride()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IAssemblyResolver>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldReturnTheLoggerWhenGetLoggerIsInvoked()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                runtime.Initialize();
                runtime.GetLogger().ShouldEqual(_mockLogger.Object);
            }

            [Fact]
            public void ShouldReturnTheScriptServiceRootWhenTheGetterIsInvoked()
            {
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType);
                runtime.Initialize();
                runtime.GetScriptServices().ShouldNotBeNull();

            }

            [Fact]
            public void ShouldRegisterTheOverriddenScriptHostFactory()
            {
                var mock = new Mock<IScriptHostFactory>();
                _overrides[typeof (IScriptHostFactory)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IScriptHostFactory>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenFilePreProcessor()
            {
                var mock = new Mock<IFilePreProcessor>();
                _overrides[typeof(IFilePreProcessor)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IFilePreProcessor>().ShouldBeType(mock.Object.GetType());
 
            }

            [Fact]
            public void ShouldRegisterTheOverriddenScriptPackResolver()
            {
                var mock = new Mock<IScriptPackResolver>();
                _overrides[typeof(IScriptPackResolver)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IScriptPackResolver>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenInstallationProvider()
            {
                var mock = new Mock<IInstallationProvider>();
                _overrides[typeof(IInstallationProvider)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IInstallationProvider>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageInstaller()
            {
                var mock = new Mock<IPackageInstaller>();
                _overrides[typeof(IPackageInstaller)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IPackageInstaller>().ShouldBeType(mock.Object.GetType());
            }

            public class TestFileSystem : FileSystem
            {
            }

            [Fact]
            public void ShouldRegisterTheOverriddenFileSystem()
            {
                _overrides[typeof(IFileSystem)] = typeof(TestFileSystem);
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IFileSystem>().ShouldBeType<TestFileSystem>();
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyUtility()
            {
                var mock = new Mock<IAssemblyUtility>();
                _overrides[typeof(IAssemblyUtility)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IAssemblyUtility>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageContainer()
            {
                var mock = new Mock<IPackageContainer>();
                _overrides[typeof(IPackageContainer)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IPackageContainer>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageAssemblyResolver()
            {
                var mock = new Mock<IPackageAssemblyResolver>();
                _overrides[typeof(IPackageAssemblyResolver)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IPackageAssemblyResolver>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyResolver()
            {
                var mock = new Mock<IAssemblyResolver>();
                _overrides[typeof(IAssemblyResolver)] = mock.Object.GetType();
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IAssemblyResolver>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyResolverInstance()
            {
                var mock = new Mock<IAssemblyResolver>();
                _overrides[typeof(IAssemblyResolver)] = mock.Object;
                var runtime = new ScriptRuntime(null, false, _mockLoggerConfigurator.Object, _mockConsole.Object, _scriptExecutorType, _scriptEngineType, _overrides);
                var container = runtime.Initialize();
                container.Resolve<IAssemblyResolver>().ShouldEqual(mock.Object);
            }




        }
    }
}
