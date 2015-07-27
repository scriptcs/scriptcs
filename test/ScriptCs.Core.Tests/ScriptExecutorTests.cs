using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
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

        public class TheEngineExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldAddReferenceToEachDestinationFile(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                var defaultReferences = ScriptExecutor.DefaultReferences;

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
                scriptExecutor.EngineExecute("", new string[] {}, new FilePreProcessorResult());

                // assert
                scriptEngine.Verify(
                    e => e.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.Is<AssemblyReferences>(x => x.Paths
                            .SequenceEqual(defaultReferences.Union(explicitReferences.Union(destPaths)))),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExecuteScriptReturnedFromFileProcessorInScriptEngine(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                ScriptExecutor scriptExecutor)
            {
                // arrange
                var code = Guid.NewGuid().ToString();

                scriptEngine.Setup(e => e.Execute(
                    code, It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));

                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                scriptExecutor.EngineExecute("", new string[] {}, new FilePreProcessorResult() {Code = code});

                // assert

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
            public void ShouldAddNamespacesFromScriptLibrary(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                Mock<ScriptExecutor> scriptExecutor
                )
            {
                scriptExecutor.Protected();
                var result = new FilePreProcessorResult();
                scriptExecutor.Setup(e => e.InjectScriptLibraries(It.IsAny<string>(), result, It.IsAny<IDictionary<string, object>>()))
                    .Callback((string p, FilePreProcessorResult r, IDictionary<string, object> s) =>
                    {
                        r.Namespaces.Add("Foo.Bar");
                    });

                scriptEngine.Setup(e => e.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));


                scriptExecutor.Object.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                scriptExecutor.Object.EngineExecute("", new string[] {}, result);

                scriptEngine.Verify(
                    e => e.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.Is<IEnumerable<string>>(x => x.Contains("Foo.Bar")),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddReferencesFromScriptLibrary(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                Mock<ScriptExecutor> scriptExecutor
                )
            {
                scriptExecutor.Protected();
                var result = new FilePreProcessorResult();
                scriptExecutor.Setup(e => e.InjectScriptLibraries(It.IsAny<string>(), result, It.IsAny<IDictionary<string, object>>()))
                    .Callback((string p, FilePreProcessorResult r, IDictionary<string, object> s) =>
                    {
                        r.References.Add("Foo.Bar");
                    });

                scriptEngine.Setup(e => e.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));


                scriptExecutor.Object.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                scriptExecutor.Object.EngineExecute("", new string[] {}, result);

                scriptEngine.Verify(
                    e => e.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.Is<AssemblyReferences>(x => x.Paths.Contains("Foo.Bar")),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddNamespaces(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                ScriptExecutor scriptExecutor)
            {
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
                scriptExecutor.EngineExecute("", new string[] {}, new FilePreProcessorResult());

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
            public void ShouldPassDefaultNamespacesToEngine(
                [Frozen] Mock<IScriptEngine> engine,
                ScriptExecutor executor)
            {
                // arrange
 
                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                executor.EngineExecute("", new string[] {}, new FilePreProcessorResult());

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
            public void ShouldPassDefaultReferencesToEngine(
                [Frozen] Mock<IScriptEngine> engine,
                ScriptExecutor executor)
            {
                // arrange
 
                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // act
                executor.EngineExecute("", new string[] {}, new FilePreProcessorResult());

                // assert
                engine.Verify(
                    i => i.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.Is<AssemblyReferences>(x =>
                            !x.Paths.Except(ScriptExecutor.DefaultReferences).Any()),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Exactly(1));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldInvokeInjectScriptLibraries(Mock<ScriptExecutor> executor)
            {
                executor.Protected();
                executor.Object.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>(), "");
                executor.Setup(e => e.InjectScriptLibraries(
                    It.IsAny<string>(), It.IsAny<FilePreProcessorResult>(), It.IsAny<IDictionary<string, object>>()));
                
                var result = new FilePreProcessorResult();

                executor.Object.EngineExecute("", new string[] { }, result);
               
                executor.Verify(e => e.InjectScriptLibraries(
                    It.IsAny<string>(), result, It.IsAny<IDictionary<string, object>>()));
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
            public void ShouldInvokeEngineExecute(
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                Mock<ScriptExecutor> executor,
                FilePreProcessorResult result
            )
            {
                executor.Protected();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(result);
                executor.Setup(
                    e => e.EngineExecute(
                        It.IsAny<string>(), 
                        It.IsAny<string[]>(), 
                        It.IsAny<FilePreProcessorResult>()));

                var args = new string[] {};

                executor.Object.Execute("", args);

                executor.Verify(
                    e => e.EngineExecute(
                        "", 
                        args, 
                        result));
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
        }

        public class TheExecuteScriptMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldInvokeEngineExecute(
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                Mock<ScriptExecutor> executor,
                FilePreProcessorResult result
            )
            {
                executor.Protected();
                preProcessor.Setup(p => p.ProcessScript(It.IsAny<string>())).Returns(result);
                executor.Setup(
                    e => e.EngineExecute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<FilePreProcessorResult>()));

                var args = new string[] { };

                executor.Object.ExecuteScript("", args);

                executor.Verify(
                    e => e.EngineExecute(
                        "workingdirectory",
                        args,
                        result));
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
                executor.InjectScriptLibraries("", _result, _state);
                _result.Code.ShouldBeEmpty();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExitIfSessionPackageScriptsInjectedIsSet(Mock<ScriptExecutor> executor)
            {
                _state["ScriptLibrariesInjected"] = null;
                executor.Protected();
                executor.Setup(e => e.LoadScriptLibraries(It.IsAny<string>())).Returns(_scriptLibrariesPreProcessorResult);
                executor.Object.InjectScriptLibraries("", _result, _state);
                _result.Code.ShouldBeEmpty();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldInjectResultCode(Mock<ScriptExecutor> executor)
            {
                executor.Protected();
                executor.Setup(e => e.LoadScriptLibraries(It.IsAny<string>())).Returns(_scriptLibrariesPreProcessorResult);
                executor.Object.InjectScriptLibraries("", _result, _state);
                _result.Code.ShouldEqual("Test" + Environment.NewLine);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddResultReferences(Mock<ScriptExecutor> executor)
            {
                executor.Protected();
                executor.Setup(e => e.LoadScriptLibraries(It.IsAny<string>())).Returns(_scriptLibrariesPreProcessorResult);
                _scriptLibrariesPreProcessorResult.References.Add("ref1");
                executor.Object.InjectScriptLibraries("", _result, _state);
                _result.References.ShouldContain("ref1");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddResultNamespaces(Mock<ScriptExecutor> executor)
            {
                executor.Protected();
                executor.Setup(e => e.LoadScriptLibraries(It.IsAny<string>())).Returns(_scriptLibrariesPreProcessorResult);
                _scriptLibrariesPreProcessorResult.Namespaces.Add("ns1");
                executor.Object.InjectScriptLibraries("", _result, _state);
                _result.Namespaces.ShouldContain("ns1");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetPackageScriptsInjectedInSession(Mock<ScriptExecutor> executor)
            {
                executor.Protected();
                executor.Setup(e => e.LoadScriptLibraries(It.IsAny<string>())).Returns(_scriptLibrariesPreProcessorResult);
                executor.Object.InjectScriptLibraries("", _result, _state);
                _state.ContainsKey("ScriptLibrariesInjected");
            }
        }

        public class TheLoadScriptLibrariesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldPreProcessTheScriptLibrariesFileIfPresent(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IFilePreProcessor> preProcessor,
                [Frozen] Mock<IScriptEngine> engine,
                [Frozen] TestLogProvider logProvider,
                [Frozen] Mock<IScriptLibraryComposer> composer)
            {
                // arrange
                fileSystem.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(true);
                var executor = new ScriptExecutor(
                    fileSystem.Object, preProcessor.Object, engine.Object, logProvider, composer.Object);

                // act
                executor.LoadScriptLibraries("");
                
                // assert
                preProcessor.Verify(p => p.ProcessFile(It.IsAny<string>()));
            }
        }

        public class TheAddReferencesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldAddReferenceToEachAssembly(ScriptExecutor executor)
            {
                // arrange
                var assembly1 = typeof(Mock).Assembly;
                var assembly2 = typeof(FrozenAttribute).Assembly;
                var assembly3 = typeof(Assert).Assembly;

                // act
                executor.AddReferences(assembly1, assembly2, assembly3, assembly3);

                // assert
                executor.References.Assemblies.ShouldContain(assembly1);
                executor.References.Assemblies.ShouldContain(assembly2);
                executor.References.Assemblies.ShouldContain(assembly3);
                executor.References.Assemblies.Count().ShouldEqual(3);
            }
        }

        public class TheRemoveReferencesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldRemoveReferenceToEachAssembly(ScriptExecutor executor)
            {
                // arrange
                var assembly1 = typeof(Mock).Assembly;
                var assembly2 = typeof(FrozenAttribute).Assembly;
                var assembly3 = typeof(Assert).Assembly;
                executor.AddReferences(assembly1, assembly2, assembly3);

                // act
                executor.RemoveReferences(assembly1, assembly2);

                // assert
                executor.References.Assemblies.ShouldNotContain(assembly1);
                executor.References.Assemblies.ShouldNotContain(assembly2);
                executor.References.Assemblies.ShouldContain(assembly3);
                executor.References.Assemblies.Count().ShouldEqual(1);
            }
        }
    }
}
