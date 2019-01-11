using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                var mockScriptExecutor = new MockScriptExecutor();
                _scriptExecutorType = mockScriptExecutor.GetType();

                var mockReplType = new Mock<IRepl>();
                _replType = mockReplType.Object.GetType();

                var mockScriptEngine = new MockScriptEngine();
                _scriptEngineType = mockScriptEngine.GetType();

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
                _runtimeServices.Container.IsRegistered<IConsole>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheScriptServices()
            {
                _runtimeServices.Container.IsRegistered<ScriptServices>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultScriptHostFactoryIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IScriptHostFactory>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultFilePreProcessorIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IFilePreProcessor>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultScriptPackResolverIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IScriptPackResolver>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultInstallationProviderIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IInstallationProvider>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageInstallerIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IPackageInstaller>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultScriptServiceRootIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<ScriptServices>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultFileSystemIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IFileSystem>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultAssemblyUtilityIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IAssemblyUtility>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageContainerIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IPackageContainer>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultPackageAssemblyResolverIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IPackageAssemblyResolver>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultAssemblyResolverIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IAssemblyResolver>().ShouldBeTrue();
            }

            [Fact]
            public void ShouldRegisterTheDefaultVisualStudioSolutionWriterIfNoOverride()
            {
                _runtimeServices.Container.IsRegistered<IVisualStudioSolutionWriter>().ShouldBeTrue();
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
                var processorList = _overrides[typeof(ILineProcessor)] as List<Type>;
                processorList.ShouldNotBeNull();
                processorList.Add(typeof(MockLineProcessor));

                var processors = _runtimeServices.Container.Resolve<IEnumerable<ILineProcessor>>();
                processors.ShouldNotBeNull();
                processors.Where(p => p.GetType() == typeof(MockLineProcessor)).ShouldNotBeEmpty();
            }

            [Fact]
            public void ShouldRegisterTheOverriddenScriptHostFactory()
            {
                _overrides[typeof(IScriptHostFactory)] = typeof(MockScriptHostFactory);
                _runtimeServices.Container.Resolve<IScriptHostFactory>().ShouldBeType(typeof(MockScriptHostFactory));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenFilePreProcessor()
            {
                var mock = new Mock<IFilePreProcessor>();
                _overrides[typeof(IFilePreProcessor)] = typeof(MockFilePreProcessor);
                _runtimeServices.Container.Resolve<IFilePreProcessor>().ShouldBeType(typeof(MockFilePreProcessor));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenScriptPackResolver()
            {
                _overrides[typeof(IScriptPackResolver)] = typeof(MockScriptPackResolver);
                _runtimeServices.Container.Resolve<IScriptPackResolver>().ShouldBeType(typeof(MockScriptPackResolver));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenInstallationProvider()
            {
                _overrides[typeof(IInstallationProvider)] = typeof(MockInstallationProvider);
                _runtimeServices.Container.Resolve<IInstallationProvider>().ShouldBeType(typeof(MockInstallationProvider));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageInstaller()
            {
                _overrides[typeof(IPackageInstaller)] = typeof(MockPackageInstaller);
                _runtimeServices.Container.Resolve<IPackageInstaller>().ShouldBeType(typeof(MockPackageInstaller));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenFileSystem()
            {
                _overrides[typeof(IFileSystem)] = typeof(MockFileSystem);
                _runtimeServices.Container.Resolve<IFileSystem>().ShouldBeType(typeof(MockFileSystem));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyUtility()
            {
                _overrides[typeof(IAssemblyUtility)] = typeof(MockAssemblyUtility);
                _runtimeServices.Container.Resolve<IAssemblyUtility>().ShouldBeType(typeof(MockAssemblyUtility));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenConsole()
            {
                _overrides[typeof(IConsole)] = typeof(MockConsole);
                _runtimeServices.Container.Resolve<IConsole>().ShouldBeType(typeof(MockConsole));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageContainer()
            {
                _overrides[typeof(IPackageContainer)] = typeof(MockPackageContainer);
                _runtimeServices.Container.Resolve<IPackageContainer>().ShouldBeType(typeof(MockPackageContainer));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenPackageAssemblyResolver()
            {
                _overrides[typeof(IPackageAssemblyResolver)] = typeof(MockPackageAssemblyResolver);
                _runtimeServices.Container.Resolve<IPackageAssemblyResolver>().ShouldBeType(typeof(MockPackageAssemblyResolver));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyResolver()
            {
                _overrides[typeof(IAssemblyResolver)] = typeof(MockAssemblyResolver);
                _runtimeServices.Container.Resolve<IAssemblyResolver>().ShouldBeType(typeof(MockAssemblyResolver));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenAssemblyResolverInstance()
            {
                var mock = new MockAssemblyResolver();
                _overrides[typeof(IAssemblyResolver)] = mock;
                _runtimeServices.Container.Resolve<IAssemblyResolver>().ShouldEqual(mock);
            }

            [Fact]
            public void ShouldRegisterTheOverriddenVisualStudioSolutionWriter()
            {
                _overrides[typeof(IVisualStudioSolutionWriter)] = typeof(MockVisualStudioSolutionWriter);
                _runtimeServices.Container.Resolve<IVisualStudioSolutionWriter>().ShouldBeType(typeof(MockVisualStudioSolutionWriter));
            }

            [Fact]
            public void ShouldRegisterTheOverriddenRepl()
            {
                var mock = new Mock<IRepl>();
                _overrides[ typeof( IRepl ) ] = mock.Object.GetType();
                _runtimeServices.Container.Resolve<IRepl>().ShouldBeType( mock.Object.GetType() );
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

            private class MockScriptEngine : IScriptEngine
            {
                public string BaseDirectory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
                public string CacheDirectory { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
                public string FileName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

                public ScriptResult Execute(string code, string[] scriptArgs, AssemblyReferences references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockScriptExecutor : IScriptExecutor
            {
                public AssemblyReferences References => throw new NotImplementedException();

                public IReadOnlyCollection<string> Namespaces => throw new NotImplementedException();

                public IScriptEngine ScriptEngine => throw new NotImplementedException();

                public IFileSystem FileSystem => throw new NotImplementedException();

                public ScriptPackSession ScriptPackSession => throw new NotImplementedException();

                public void AddReferences(params Assembly[] references)
                {
                    throw new NotImplementedException();
                }

                public void AddReferences(params string[] references)
                {
                    throw new NotImplementedException();
                }

                public ScriptResult Execute(string script, params string[] scriptArgs)
                {
                    throw new NotImplementedException();
                }

                public ScriptResult ExecuteScript(string script, params string[] scriptArgs)
                {
                    throw new NotImplementedException();
                }

                public void ImportNamespaces(params string[] namespaces)
                {
                    throw new NotImplementedException();
                }

                public void Initialize(IEnumerable<string> paths, IEnumerable<IScriptPack> scriptPacks, params string[] scriptArgs)
                {
                    throw new NotImplementedException();
                }

                public void RemoveNamespaces(params string[] namespaces)
                {
                    throw new NotImplementedException();
                }

                public void RemoveReferences(params Assembly[] references)
                {
                    throw new NotImplementedException();
                }

                public void RemoveReferences(params string[] references)
                {
                    throw new NotImplementedException();
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }

                public void Terminate()
                {
                    throw new NotImplementedException();
                }
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

            private class MockLineProcessor : ILineProcessor
            {
                public bool ProcessLine(IFileParser parser, FileParserContext context, string line, bool isBeforeCode)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockScriptHostFactory : IScriptHostFactory
            {
                public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockFilePreProcessor : IFilePreProcessor
            {
                public void ParseFile(string path, FileParserContext context)
                {
                    throw new NotImplementedException();
                }

                public void ParseScript(List<string> scriptLines, FileParserContext context)
                {
                    throw new NotImplementedException();
                }

                public FilePreProcessorResult ProcessFile(string path)
                {
                    throw new NotImplementedException();
                }

                public FilePreProcessorResult ProcessScript(string script)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockScriptPackResolver : IScriptPackResolver
            {
                public IEnumerable<IScriptPack> GetPacks()
                {
                    throw new NotImplementedException();
                }
            }

            private class MockInstallationProvider : IInstallationProvider
            {
                public IEnumerable<string> GetRepositorySources(string path)
                {
                    throw new NotImplementedException();
                }

                public void Initialize()
                {
                    throw new NotImplementedException();
                }

                public void InstallPackage(IPackageReference packageId, bool allowPreRelease = false)
                {
                    throw new NotImplementedException();
                }

                public bool IsInstalled(IPackageReference packageId, bool allowPreRelease = false)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockPackageInstaller : IPackageInstaller
            {
                public void InstallPackages(IEnumerable<IPackageReference> packageIds, bool allowPreRelease = false)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockAssemblyUtility : IAssemblyUtility
            {
                public AssemblyName GetAssemblyName(string path)
                {
                    throw new NotImplementedException();
                }

                public bool IsManagedAssembly(string path)
                {
                    throw new NotImplementedException();
                }

                public Assembly Load(AssemblyName assemblyRef)
                {
                    throw new NotImplementedException();
                }

                public Assembly LoadFile(string path)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockConsole : IConsole
            {
                public ConsoleColor ForegroundColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

                public int Width => throw new NotImplementedException();

                public void Clear()
                {
                    throw new NotImplementedException();
                }

                public void Exit()
                {
                    throw new NotImplementedException();
                }

                public string ReadLine(string prompt)
                {
                    throw new NotImplementedException();
                }

                public void ResetColor()
                {
                    throw new NotImplementedException();
                }

                public void Write(string value)
                {
                    throw new NotImplementedException();
                }

                public void WriteLine()
                {
                    throw new NotImplementedException();
                }

                public void WriteLine(string value)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockPackageContainer : IPackageContainer
            {
                public void CreatePackageFile()
                {
                    throw new NotImplementedException();
                }

                public IPackageObject FindPackage(string path, IPackageReference packageReference)
                {
                    throw new NotImplementedException();
                }

                public IEnumerable<IPackageReference> FindReferences(string path)
                {
                    throw new NotImplementedException();
                }
            }

            private class MockPackageAssemblyResolver : IPackageAssemblyResolver
            {
                public IEnumerable<string> GetAssemblyNames(string workingDirectory)
                {
                    throw new NotImplementedException();
                }

                public IEnumerable<IPackageReference> GetPackages(string workingDirectory)
                {
                    throw new NotImplementedException();
                }

                public void SavePackages()
                {
                    throw new NotImplementedException();
                }
            }

            private class MockAssemblyResolver : IAssemblyResolver
            {
                public IEnumerable<string> GetAssemblyPaths(string path, bool binariesOnly = false)
                {
                    throw new NotImplementedException();
                }
            }
            private class MockVisualStudioSolutionWriter : IVisualStudioSolutionWriter
            {
                public string WriteSolution(IFileSystem fs, string script, IVisualStudioSolution solution, IList<ProjectItem> nestedItems = null)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
