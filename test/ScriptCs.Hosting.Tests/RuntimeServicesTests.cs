using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class RuntimeServicesTests
    {
        public class TheContainerProperty
        {
            private readonly Mock<IConsole> _mockConsole = new Mock<IConsole>();
            private readonly Type _scriptExecutorType;
            private readonly Type _replType;
            private readonly Type _scriptEngineType;
            private readonly TestLogProvider _logProvider = new TestLogProvider();
            private readonly IDictionary<Type, object> _overrides = new Dictionary<Type, object>();
            private readonly RuntimeServices _runtimeServices;

            public TheContainerProperty()
            {
                _overrides[typeof(ILineProcessor)] = new List<Type>();
                var mockScriptExecutorType = new Mock<IScriptExecutor>();
                _scriptExecutorType = mockScriptExecutorType.Object.GetType();

                var mockReplType = new Mock<IRepl>();
                _replType = mockReplType.Object.GetType();

                var mockScriptEngineType = new Mock<IScriptEngine>();
                _scriptEngineType = mockScriptEngineType.Object.GetType();

                var initializationServices = new InitializationServices(_logProvider, _overrides);
                _runtimeServices = new RuntimeServices(
                    _logProvider,
                    _overrides,
                    _mockConsole.Object,
                    _scriptEngineType,
                    _scriptExecutorType,
                    _replType,
                    false,
                    initializationServices,
                    "script.csx");
            }

            [Fact]
            public void ShouldRegisterTheLoggerInstance()
            {
                var logProvider = _runtimeServices.Container.Resolve<ILogProvider>();
                logProvider.ShouldEqual(_logProvider);
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
            public void ShouldRegisterTheDefaultVisualStudioSolutionWriterIfNoOverride()
            {
                _runtimeServices.Container.Resolve<IVisualStudioSolutionWriter>().ShouldNotBeNull();
            }

            [Fact]
            public void ShouldRegisterTheDefaultLineProcessors()
            {
                var processors = _runtimeServices.Container.Resolve<IEnumerable<ILineProcessor>>();
                processors.ShouldNotBeNull();
                processors = processors.ToArray();
                processors.Where(p => p is IUsingLineProcessor).ShouldNotBeEmpty();
                processors.Where(p => p is IReferenceLineProcessor).ShouldNotBeEmpty();
                processors.Where(p => p is ILoadLineProcessor).ShouldNotBeEmpty();
                processors.Where(p => p is IShebangLineProcessor).ShouldNotBeEmpty();
            }

            [Fact]
            public void ShouldRegisterACustomLineProcessor()
            {
                var mock = new Mock<ILineProcessor>();
                var processorList = _overrides[typeof(ILineProcessor)] as List<Type>;
                processorList.ShouldNotBeNull();
                processorList.Add(mock.Object.GetType());

                var processors = _runtimeServices.Container.Resolve<IEnumerable<ILineProcessor>>();
                processors.ShouldNotBeNull();
                processors.Where(p => p.GetType() == mock.Object.GetType()).ShouldNotBeEmpty();
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
                var mock = new MockFileSystem();
                _overrides[typeof(IFileSystem)] = mock.GetType();
                _runtimeServices.Container.Resolve<IFileSystem>().ShouldBeType(mock.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyUtility()
            {
                var mock = new Mock<IAssemblyUtility>();
                _overrides[typeof(IAssemblyUtility)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IAssemblyUtility>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldRegisterTheOverriddenConsole()
            {
                var mock = new Mock<IConsole>();
                _overrides[typeof(IConsole)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IConsole>().ShouldBeType(mock.Object.GetType());
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

            [Fact]
            public void ShouldRegisterTheOverriddenVisualStudioSolutionWriter()
            {
                var mock = new Mock<IVisualStudioSolutionWriter>();
                _overrides[typeof (IVisualStudioSolutionWriter)] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IVisualStudioSolutionWriter>().ShouldBeType(mock.Object.GetType());
            }

            [Fact]
            public void ShouldLogOnDebugAnAssemblyLoadFailure()
            {
                // arrange
                var mockResolver = new Mock<IAssemblyResolver>();
                mockResolver.Setup(a => a.GetAssemblyPaths(It.IsAny<string>(), false)).Returns(new[] { "/foo.dll" });
                _overrides[typeof(IAssemblyResolver)] = mockResolver.Object;

                var mockAssemblyUtility = new Mock<IAssemblyUtility>();
                mockAssemblyUtility.Setup(a => a.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                _overrides[typeof(IAssemblyUtility)] = mockAssemblyUtility.Object;

                var initializationServices = new InitializationServices(_logProvider, _overrides);
                var runtimeServices = new RuntimeServices(
                    _logProvider,
                    _overrides,
                    _mockConsole.Object,
                    _scriptEngineType,
                    _scriptExecutorType,
                    _replType,
                    true,
                    initializationServices,
                    "script.csx");

                // act
                var container = runtimeServices.Container;

                // assert
                _logProvider.Output.ShouldContain(
                    "DEBUG: Failure loading assembly: /foo.dll. Exception: Could not load file or assembly 'foo.dll' or one of its dependencies. The system cannot find the file specified.");
            }

            [Fact]
            public void ShouldLogAGeneralWarningOnAnAssemblyLoadFailureWhenRunningScript()
            {
                // arrange
                var mockResolver = new Mock<IAssemblyResolver>();
                mockResolver.Setup(a => a.GetAssemblyPaths(It.IsAny<string>(), false)).Returns(new[] { "/foo.dll" });
                _overrides[typeof(IAssemblyResolver)] = mockResolver.Object;

                var mockAssemblyUtility = new Mock<IAssemblyUtility>();
                mockAssemblyUtility.Setup(a => a.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                _overrides[typeof(IAssemblyUtility)] = mockAssemblyUtility.Object;

                var initializationServices = new InitializationServices(_logProvider, _overrides);
                var runtimeServices = new RuntimeServices(
                    _logProvider,
                    _overrides,
                    _mockConsole.Object,
                    _scriptEngineType,
                    _scriptExecutorType,
                    _replType,
                    true,
                    initializationServices,
                    "script.csx");

                // act
                var container = runtimeServices.Container;

                // assert
                _logProvider.Output.ShouldContain(
                    "WARN: Some assemblies failed to load. Launch with '-loglevel debug' to see the details");
            }

            [Fact]
            public void ShouldLogAGeneralWarningOnAnAssemblyLoadFailureWhenRunningInRepl()
            {
                // arrange
                var mockResolver = new Mock<IAssemblyResolver>();
                mockResolver.Setup(a => a.GetAssemblyPaths(It.IsAny<string>(), false)).Returns(new[] { "/foo.dll" });
                _overrides[typeof(IAssemblyResolver)] = mockResolver.Object;

                var mockAssemblyUtility = new Mock<IAssemblyUtility>();
                mockAssemblyUtility.Setup(a => a.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                _overrides[typeof (IAssemblyUtility)] = mockAssemblyUtility.Object;

                var initializationServices = new InitializationServices(_logProvider, _overrides);
                var runtimeServices = new RuntimeServices(
                    _logProvider,
                    _overrides,
                    _mockConsole.Object,
                    _scriptEngineType,
                    _scriptExecutorType,
                    _replType, true,
                    initializationServices,
                    "");

                // act
                var container = runtimeServices.Container;

                // assert
                _logProvider.Output.ShouldContain(
                    "WARN: Some assemblies failed to load. Launch with '-repl -loglevel debug' to see the details");
            }

            [Fact]
            public void ShouldResolveAssembliesBasedOnScriptWorkingDirectory()
            {
                // arrange
                var fsmock = new Mock<IFileSystem>();
                fsmock.Setup(a => a.GetWorkingDirectory(It.IsAny<string>())).Returns("c:/scripts");

                var resolvermock = new Mock<IAssemblyResolver>();
                resolvermock.Setup(a => a.GetAssemblyPaths("c:/scripts", false)).Returns(new[] { "foo.dll" });

                _overrides[typeof(IFileSystem)] = fsmock.Object;
                _overrides[typeof(IAssemblyResolver)] = resolvermock.Object;

                var initializationServices = new InitializationServices(_logProvider, _overrides);
                var runtimeServices = new RuntimeServices(
                    _logProvider,
                    _overrides,
                    _mockConsole.Object,
                    _scriptEngineType,
                    _scriptExecutorType,
                    _replType,
                    true,
                    initializationServices,
                    "c:/scriptcs/script.csx");

                // act
                var container = runtimeServices.Container;

                // assert
                resolvermock.Verify(x => x.GetAssemblyPaths("c:/scripts", false), Times.Exactly(1));
            }

            private class MockFileSystem : IFileSystem
            {
                public IEnumerable<string> EnumerateFiles(
                    string dir, string search, SearchOption searchOption = SearchOption.AllDirectories)
                {
                    throw new NotImplementedException();
                }

                public IEnumerable<string> EnumerateDirectories(
                    string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
                {
                    throw new NotImplementedException();
                }

                public IEnumerable<string> EnumerateFilesAndDirectories(
                    string dir, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
                {
                    throw new NotImplementedException();
                }

                public void Copy(string source, string dest, bool overwrite)
                {
                    throw new NotImplementedException();
                }

                public void CopyDirectory(string source, string dest, bool overwrite)
                {
                    throw new NotImplementedException();
                }

                public bool DirectoryExists(string path)
                {
                    throw new NotImplementedException();
                }

                public void CreateDirectory(string path, bool hidden = false)
                {
                    throw new NotImplementedException();
                }

                public void DeleteDirectory(string path)
                {
                    throw new NotImplementedException();
                }

                public string ReadFile(string path)
                {
                    throw new NotImplementedException();
                }

                public string[] ReadFileLines(string path)
                {
                    throw new NotImplementedException();
                }

                public DateTime GetLastWriteTime(string file)
                {
                    throw new NotImplementedException();
                }

                public bool IsPathRooted(string path)
                {
                    throw new NotImplementedException();
                }

                public string GetFullPath(string path)
                {
                    throw new NotImplementedException();
                }

                public string TempPath { get; private set; }

                public string CurrentDirectory
                {
                    get { throw new NotImplementedException(); }
                    set { throw new NotImplementedException(); }
                }

                public string NewLine
                {
                    get { throw new NotImplementedException(); }
                }

                public string GetWorkingDirectory(string path)
                {
                    throw new NotImplementedException();
                }

                public void Move(string source, string dest)
                {
                    throw new NotImplementedException();
                }

                public void MoveDirectory(string source, string dest)
                {
                    throw new NotImplementedException();
                }

                public bool FileExists(string path)
                {
                    throw new NotImplementedException();
                }

                public void FileDelete(string path)
                {
                    throw new NotImplementedException();
                }

                public IEnumerable<string> SplitLines(string value)
                {
                    throw new NotImplementedException();
                }

                public void WriteToFile(string path, string text)
                {
                    throw new NotImplementedException();
                }

                public Stream CreateFileStream(string filePath, FileMode mode)
                {
                    throw new NotImplementedException();
                }

                public void WriteAllBytes(string filePath, byte[] bytes)
                {
                    throw new NotImplementedException();
                }

                public string GlobalFolder
                {
                    get { throw new NotImplementedException(); }
                }

                public string HostBin
                {
                    get { throw new NotImplementedException(); }
                }

                public string BinFolder
                {
                    get { return "bin"; }
                }

                public string DllCacheFolder
                {
                    get { throw new NotImplementedException(); }
                }

                public string PackagesFile
                {
                    get { return "packages.config"; }
                }

                public string PackagesFolder
                {
                    get { return "packages"; }
                }

                public string NugetFile
                {
                    get { throw new NotImplementedException(); }
                }

                public string GlobalOptsFile
                {
                    get { throw new NotImplementedException(); }
                }

                public string PackageScriptsFile
                {
                    get { throw new NotImplementedException(); }
                }
            }
        }
    }
}
