using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using Moq.Protected;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class ScriptExecutorTests
    {
        public class TheInitializeMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldSetEngineBaseDirectoryBasedOnCurrentDirectoryAndBinFolder(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                preProcessor.Setup(x => x.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());
                scriptEngine.SetupProperty(e => e.BaseDirectory);

                // act
                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // assert
                scriptEngine.Object.BaseDirectory.ShouldEqual(
                    Path.Combine(fileSystem.Object.CurrentDirectory, "scriptcs_bin"));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetEngineDllCacheDirectoryBasedOnCurrentDirectory(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                preProcessor.Setup(x => x.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());
                scriptEngine.SetupProperty(e => e.CacheDirectory);

                // act
                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // assert
                scriptEngine.Object.CacheDirectory.ShouldEqual(
                    Path.Combine(fileSystem.Object.CurrentDirectory, ".scriptcs_cache"));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldInitializeScriptPacks(
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                [Frozen] Mock<IScriptPack> scriptPack,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                scriptPack.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());

                // act
                scriptExecutor.Initialize(Enumerable.Empty<string>(), new[] { scriptPack.Object });

                // assert
                scriptPack.Verify(p => p.Initialize(It.IsAny<IScriptPackSession>()));
            }
            
            [Theory, ScriptCsAutoData]
            public void ShouldPreProcessThePackageScriptIfPresent(
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                [Frozen] Mock<IFileSystem> fileSystem,
                ScriptExecutor scriptExecutor)
            {
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());
                fileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                preProcessor.Verify(p=> p.ProcessFile(It.IsAny<string>()));
            }
        }

        public class TheTerminateMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldTerminateScriptPacksWhenTerminateIsCalled(
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                [Frozen] Mock<IScriptPack> scriptPack,
                ScriptExecutor executor)
            {
                // arrange
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                scriptPack.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());
                scriptPack.Setup(p => p.Terminate());

                executor.Initialize(Enumerable.Empty<string>(), new[] { scriptPack.Object });
                executor.Execute("script.csx");

                // act
                executor.Terminate();

                // assert
                scriptPack.Verify(p => p.Terminate());
            }
        }

        public class TheExecuteMethod
        {
            private readonly string _tempPath;

            public TheExecuteMethod()
            {
                _tempPath = Path.GetTempPath();
            }

            [Theory, ScriptCsAutoData]
            public void ConstructsAbsolutePathBeforePreProcessingFile(
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                [Frozen] Mock<IFileSystem> fileSystem,
                ScriptExecutor executor)
            {
                // arrange
                fileSystem.Setup(f => f.CurrentDirectory).Returns(Path.Combine(_tempPath, "my_script"));
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>()))
                    .Returns(Path.Combine(_tempPath, "my_script"));
                
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                executor.Execute("script.csx");

                // assert
                preProcessor.Verify(p => p.ProcessFile(
                    Path.Combine(_tempPath, "my_script", "script.csx")));
            }

            [Theory, ScriptCsAutoData]
            public void DoNotChangePathIfAbsolute(
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                [Frozen] Mock<IFileSystem> fileSystem,
                ScriptExecutor executor)
            {
                // arrange
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>()))
                    .Returns(Path.Combine(_tempPath, "my_script"));

                fileSystem.Setup(f => f.CurrentDirectory).Returns(Path.Combine(_tempPath, "my_script"));

                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                executor.Execute("script.csx");

                // assert
                preProcessor.Verify(p => p.ProcessFile(
                    Path.Combine(_tempPath, "my_script", "script.csx")));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExecuteScriptReturnedFromFileProcessorInScriptEngineWhenExecuteIsInvoked(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                var currentDirectory = fileSystem.Object.CurrentDirectory;
                var scriptName = "script.csx";
                var code = Guid.NewGuid().ToString();

                preProcessor.Setup(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName)))
                    .Returns(new FilePreProcessorResult { Code = code }).Verifiable();

                scriptEngine.Setup(e => e.Execute(
                    code, It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));

                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                scriptExecutor.Execute(scriptName);

                // assert
                preProcessor.Verify(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName)), Times.Once());

                scriptEngine.Verify(
                    s => s.Execute(
                        code,
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExecuteScriptWhenExecuteScriptIsInvoked(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor scriptExecutor)
            {
                var script = "var a=1;";
                preProcessor.Setup(fs => fs.ProcessScript(script))
                    .Returns(new FilePreProcessorResult { Code = script }).Verifiable();

                var code = Guid.NewGuid().ToString();
                scriptEngine.Setup(e => e.Execute(
                    code,
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));

                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                scriptExecutor.ExecuteScript(script);

                // assert
                preProcessor.Verify(fs => fs.ProcessScript(script), Times.Once());

                scriptEngine.Verify(
                    s => s.Execute(
                        script,
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddReferenceToEachDestinationFile(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                var defaultReferences = ScriptExecutor.DefaultReferences;
                preProcessor.Setup(x => x.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());

                var currentDirectory = @"C:\";
                var destinationFilePath1 = Path.Combine(currentDirectory, @"bin\fileName1.cs");
                var destinationFilePath2 = Path.Combine(currentDirectory, @"bin\fileName2.cs");
                var destinationFilePath3 = Path.Combine(currentDirectory, @"bin\fileName3.cs");
                var destinationFilePath4 = Path.Combine(currentDirectory, @"bin\fileName4.cs");

                var destPaths = new[]
                {
                    "System", 
                    "System.Core",
                    destinationFilePath1,
                    destinationFilePath2,
                    destinationFilePath3,
                    destinationFilePath4,
                };

                scriptEngine.Setup(e => e.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));

                scriptExecutor.AddReferences("a");
                scriptExecutor.AddReferences(new[] { "a", "a", "b", "c", "d" });
                scriptExecutor.AddReference<FactAttribute>();
                scriptExecutor.AddReferences(typeof(TheInitializeMethod));
                var explicitReferences = new[]
                {
                    "a",
                    "b",
                    "c",
                    "d", 
                    typeof(FactAttribute).Assembly.Location,
                    typeof(TheInitializeMethod).Assembly.Location,
                };

                scriptExecutor.Initialize(destPaths, Enumerable.Empty<IScriptPack>());

                // act
                scriptExecutor.Execute("script.csx");

                // assert
                scriptEngine.Verify(
                    e => e.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.Is<AssemblyReferences>(x => x.PathReferences
                            .SequenceEqual(defaultReferences.Union(explicitReferences.Union(destPaths)))),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetEngineFileName(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                scriptEngine.SetupProperty(e => e.FileName);
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());
                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                var scriptName = "script.csx";

                // act
                scriptExecutor.Execute(scriptName);

                // assert
                scriptEngine.Object.FileName.ShouldEqual(scriptName);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddNamespaces(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                preProcessor.Setup(x => x.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());

                scriptEngine.Setup(e => e.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));

                scriptExecutor.ImportNamespaces("a");
                scriptExecutor.ImportNamespaces(new[] { "a", "a", "b", "c", "d" }.ToArray());
                scriptExecutor.ImportNamespace<FactAttribute>();
                scriptExecutor.ImportNamespaces(typeof(TheInitializeMethod));
                var explicitNamespaces = new[]
                {
                    "a",
                    "b",
                    "c",
                    "d",
                    typeof(FactAttribute).Namespace, 
                    typeof(TheInitializeMethod).Namespace
                };

                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                scriptExecutor.Execute("script.csx");

                // assert
                scriptEngine.Verify(
                    e => e.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.Is<IEnumerable<string>>(x =>
                            x.SequenceEqual(ScriptExecutor.DefaultNamespaces.Union(explicitNamespaces))),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ExecutorShouldPassDefaultNamespacesToEngine(
                [Frozen] Mock<IScriptEngine> engine,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor executor)
            {
                // arrange
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                executor.Execute("script.csx");

                // assert
                engine.Verify(
                    i => i.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.Is<IEnumerable<string>>(x => !x.Except(ScriptExecutor.DefaultNamespaces).Any()),
                        It.IsAny<ScriptPackSession>()),
                    Times.Exactly(1));
            }

            [Theory, ScriptCsAutoData]
            public void ExecutorShouldPassDefaultReferencesToEngine(
                [Frozen] Mock<IScriptEngine> engine,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                ScriptExecutor executor)
            {
                // arrange
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                executor.Execute("script.csx");

                // assert
                engine.Verify(
                    i => i.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.Is<AssemblyReferences>(x =>
                            !x.PathReferences.Except(ScriptExecutor.DefaultReferences).Any()),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Exactly(1));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldInvokeInjectScriptLibraries(Mock<ScriptExecutor> executor)
            {
                executor.Protected();
                executor.Object.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>(), "");
                executor.Setup(e => e.InjectScriptLibraries(It.IsAny<FilePreProcessorResult>(), It.IsAny<FilePreProcessorResult>(), It.IsAny<IDictionary<string,object>>()));
                executor.Object.Execute("");
                executor.Verify(e=>e.InjectScriptLibraries(It.IsAny<FilePreProcessorResult>(), It.IsAny<FilePreProcessorResult>(), It.IsAny<IDictionary<string,object>>()));
            }
        }

        public class TheInjectScriptLibrariesMethod
        {
            private IDictionary<string, object> _state = new Dictionary<string, object>();
            private FilePreProcessorResult _result = new FilePreProcessorResult();
            private FilePreProcessorResult _scriptLibrariesPreProcessorResult = new FilePreProcessorResult();

            public TheInjectScriptLibrariesMethod()
            {
                _scriptLibrariesPreProcessorResult.Code = "Test";
                _result.Code = "";
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExitIfPreProcessorResultIsNull(ScriptExecutor executor)
            {
                executor.InjectScriptLibraries(_result, null, null);
                _result.Code.ShouldBeEmpty();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExitIfSessionPackageScriptsInjectedIsSet(ScriptExecutor executor)
            {
                _state["ScriptLibrariesInjected"] = null;
                executor.InjectScriptLibraries(_result, _scriptLibrariesPreProcessorResult, _state);
                _result.Code.ShouldBeEmpty();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldInjectResultCode(ScriptExecutor executor)
            {
                executor.InjectScriptLibraries(_result, _scriptLibrariesPreProcessorResult, _state);
                _result.Code.ShouldEqual(Environment.NewLine + "Test");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddResultReferences(ScriptExecutor executor)
            {
                _scriptLibrariesPreProcessorResult.References.Add("ref1");
                executor.InjectScriptLibraries(_result, _scriptLibrariesPreProcessorResult, _state);
                _result.References.ShouldContain("ref1");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddResultNamespaces(ScriptExecutor executor)
            {
                _scriptLibrariesPreProcessorResult.Namespaces.Add("ns1");
                executor.InjectScriptLibraries(_result, _scriptLibrariesPreProcessorResult, _state);
                _result.Namespaces.ShouldContain("ns1");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetPackageScriptsInjectedInSession(ScriptExecutor executor)
            {
                executor.InjectScriptLibraries(_result, _scriptLibrariesPreProcessorResult, _state);
                _state.ContainsKey("ScriptLibrariesInjected");
            }
        }

        public class TheAddReferencesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldAddReferenceToEachAssembly(ScriptExecutor executor)
            {
                // arrange
                var calling = Assembly.GetCallingAssembly();
                var executing = Assembly.GetExecutingAssembly();
                var entry = Assembly.GetEntryAssembly();

                // act
                executor.AddReferences(calling, executing, entry, entry);

                // assert
                executor.References.Assemblies.ShouldContain(calling);
                executor.References.Assemblies.ShouldContain(executing);
                executor.References.Assemblies.ShouldContain(entry);
                executor.References.Assemblies.Count.ShouldEqual(3);
            }
        }

        public class TheRemoveReferencesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldRemoveReferenceToEachAssembly(ScriptExecutor executor)
            {
                // arrange
                var calling = Assembly.GetCallingAssembly();
                var executing = Assembly.GetExecutingAssembly();
                var entry = Assembly.GetEntryAssembly();
                executor.AddReferences(calling, executing, entry);

                // act
                executor.RemoveReferences(calling, executing);

                // assert
                executor.References.Assemblies.ShouldNotContain(calling);
                executor.References.Assemblies.ShouldNotContain(executing);
                executor.References.Assemblies.ShouldContain(entry);
                executor.References.Assemblies.Count.ShouldEqual(1);
            }
        }
    }
}
