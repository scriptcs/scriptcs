using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
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
            public void ShouldSetEngineBaseDirectoryBasedOnCurrentDirectoryAndBinFolder([Frozen] Mock<IScriptEngine> scriptEngine, [Frozen] Mock<IFileSystem> fileSystem,
                                                                                                                                     [Frozen] Mock<IFilePreProcessor> preProcessor, ScriptExecutor scriptExecutor)
            {
                // arrange
                preProcessor.Setup(x => x.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());

                var currentDirectory = @"C:\";
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                scriptEngine.SetupProperty(e => e.BaseDirectory);

                var paths = new string[0];
                IEnumerable<IScriptPack> recipes = Enumerable.Empty<IScriptPack>();

                // act
                scriptExecutor.Initialize(paths, recipes);

                // assert
                string expectedBaseDirectory = Path.Combine(currentDirectory, "bin");
                expectedBaseDirectory.ShouldEqual(scriptEngine.Object.BaseDirectory);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldInitializeScriptPacks([Frozen] Mock<IFilePreProcessor> preProcessor, [Frozen] Mock<IFileSystem> fileSystem,
                                                                     [Frozen] Mock<IScriptPack> scriptPack1, ScriptExecutor scriptExecutor)
            {
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                scriptPack1.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());

                // act
                scriptExecutor.Initialize(Enumerable.Empty<string>(), new[] { scriptPack1.Object });

                // assert
                scriptPack1.Verify(p => p.Initialize(It.IsAny<IScriptPackSession>()));
            }
        }

        public class TheTerminateMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldTerminateScriptPacksWhenTerminateIsCalled([Frozen] Mock<IFilePreProcessor> preProcessor, [Frozen] Mock<IFileSystem> fileSystem,
                                                                                                           [Frozen] Mock<IScriptPack> scriptPack1, ScriptExecutor executor)
            {
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                scriptPack1.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());
                scriptPack1.Setup(p => p.Terminate());

                // act
                executor.Initialize(Enumerable.Empty<string>(), new[] { scriptPack1.Object });
                executor.ExecuteFile("script.csx");
                executor.Terminate();

                // assert
                scriptPack1.Verify(p => p.Terminate());
            }
        }

        public class TheExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ConstructsAbsolutePathBeforePreProcessingFile([Frozen] Mock<IFilePreProcessor> preProcessor, [Frozen] Mock<IFileSystem> fileSystem, ScriptExecutor executor)
            {
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                executor.ExecuteFile("script.csx");
                preProcessor.Verify(p => p.ProcessFile(@"c:\my_script\script.csx"));
            }

            [Theory, ScriptCsAutoData]
            public void DoNotChangePathIfAbsolute([Frozen] Mock<IFilePreProcessor> preProcessor, [Frozen] Mock<IFileSystem> fileSystem, ScriptExecutor executor)
            {
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                executor.ExecuteFile("script.csx");

                preProcessor.Verify(p => p.ProcessFile(@"c:\my_script\script.csx"));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExecuteScriptReturnedFromFileProcessorInScriptEngineWhenExecuteIsInvoked([Frozen] Mock<IScriptEngine> scriptEngine, [Frozen] Mock<IFileSystem> fileSystem,
                                                                                                                                     [Frozen] Mock<IFilePreProcessor> preProcessor, ScriptExecutor scriptExecutor)
            {
                string code = Guid.NewGuid().ToString();

                var currentDirectory = @"C:\";
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                var scriptName = "script.csx";
                var paths = new string[0];
                var recipes = Enumerable.Empty<IScriptPack>();

                preProcessor.Setup(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName))).Returns(new FilePreProcessorResult { Code = code }).Verifiable();
                scriptEngine.Setup(e => e.Execute(code, It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()));

                // act
                scriptExecutor.Initialize(paths, recipes);
                scriptExecutor.ExecuteFile(scriptName);

                // assert
                preProcessor.Verify(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName)), Times.Once());

                scriptEngine.Verify(s => s.Execute(code, It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExecuteScriptWhenExecuteScriptIsInvoked([Frozen] Mock<IScriptEngine> scriptEngine, [Frozen] Mock<IFileSystem> fileSystem,
                                                                                                                                     [Frozen] Mock<IFilePreProcessor> preProcessor, ScriptExecutor scriptExecutor)
            {
                string code = Guid.NewGuid().ToString();

                var currentDirectory = @"C:\";
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                var paths = new string[0];
                var recipes = Enumerable.Empty<IScriptPack>();
                var script = "var a=1;";
                preProcessor.Setup(fs => fs.ProcessCode(script)).Returns(new FilePreProcessorResult { Code = script }).Verifiable();
                scriptEngine.Setup(e => e.Execute(code, It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()));

                // act
                scriptExecutor.Initialize(paths, recipes);
                scriptExecutor.ExecuteCode(script);

                // assert
                preProcessor.Verify(fs => fs.ProcessCode(script), Times.Once());

                scriptEngine.Verify(s => s.Execute(script, It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddReferenceToEachDestinationFile([Frozen] Mock<IScriptEngine> scriptEngine, [Frozen] Mock<IFileSystem> fileSystem,
                                                                                            [Frozen] Mock<IFilePreProcessor> preProcessor, ScriptExecutor scriptExecutor)
            {
                // arrange
                var defaultReferences = ScriptExecutor.DefaultReferences;
                preProcessor.Setup(x => x.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());

                var currentDirectory = @"C:\";
                var destinationFilePath1 = Path.Combine(currentDirectory, @"bin\fileName1.cs");
                var destinationFilePath2 = Path.Combine(currentDirectory, @"bin\fileName2.cs");
                var destinationFilePath3 = Path.Combine(currentDirectory, @"bin\fileName3.cs");
                var destinationFilePath4 = Path.Combine(currentDirectory, @"bin\fileName4.cs");

                var scriptName = "script.csx";

                var paths = new string[] { destinationFilePath1, destinationFilePath2, destinationFilePath3, destinationFilePath4 };

                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);

                var destPaths = new string[] { "System", "System.Core", destinationFilePath1, destinationFilePath2, destinationFilePath3, destinationFilePath4 };

                scriptEngine.Setup(e => e.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()));

                scriptExecutor.AddReferences("a");
                scriptExecutor.AddReferences(new[] { "a", "a", "b", "c", "d" });
                scriptExecutor.AddReference<FactAttribute>();
                scriptExecutor.AddReferences(typeof(TheInitializeMethod));
                var explicitReferences = new[] { "a", "b", "c", "d", typeof(FactAttribute).Assembly.Location, typeof(TheInitializeMethod).Assembly.Location };

                // act
                scriptExecutor.Initialize(destPaths, Enumerable.Empty<IScriptPack>());
                scriptExecutor.ExecuteFile(scriptName);

                // assert
                scriptEngine.Verify(e => e.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.Is<IEnumerable<string>>(x => x.SequenceEqual(defaultReferences.Union(explicitReferences.Union(destPaths)))), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetEngineFileName([Frozen] Mock<IScriptEngine> scriptEngine, [Frozen] Mock<IFileSystem> fileSystem,
                                                                [Frozen] Mock<IFilePreProcessor> preProcessor, ScriptExecutor scriptExecutor)
            {
                // arrange
                const string CurrentDirectory = @"c:\scriptcs";

                scriptEngine.SetupProperty(e => e.FileName);

                fileSystem.Setup(fs => fs.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(CurrentDirectory);

                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());

                const string ScriptName = "script.csx";

                // act
                scriptExecutor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                scriptExecutor.ExecuteFile(ScriptName);

                // assert
                scriptEngine.Object.FileName.ShouldEqual(ScriptName);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddNamespaces([Frozen] Mock<IScriptEngine> scriptEngine, [Frozen] Mock<IFileSystem> fileSystem,
                                                                [Frozen] Mock<IFilePreProcessor> preProcessor, ScriptExecutor scriptExecutor)
            {
                // arrange
                preProcessor.Setup(x => x.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult());

                var currentDirectory = @"C:\";

                var scriptName = "script.csx";

                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);

                scriptEngine.Setup(e => e.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()));

                scriptExecutor.ImportNamespaces("a");
                scriptExecutor.ImportNamespaces(new[] { "a", "a", "b", "c", "d" }.ToArray());
                scriptExecutor.ImportNamespace<FactAttribute>();
                scriptExecutor.ImportNamespaces(typeof(TheInitializeMethod));
                var explicitNamespaces = new[] { "a", "b", "c", "d", typeof(FactAttribute).Namespace, typeof(TheInitializeMethod).Namespace };

                // act
                scriptExecutor.Initialize(new string[0], Enumerable.Empty<IScriptPack>());
                scriptExecutor.ExecuteFile(scriptName);

                // assert
                scriptEngine.Verify(e => e.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.Is<IEnumerable<string>>(x => x.SequenceEqual(ScriptExecutor.DefaultNamespaces.Union(explicitNamespaces))), It.IsAny<ScriptPackSession>()), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ExecutorShouldPassDefaultNamespacesToEngine([Frozen] Mock<IScriptEngine> engine, [Frozen] Mock<IFileSystem> fileSystem,
                                                                                                       [Frozen] Mock<IFilePreProcessor> preProcessor, ScriptExecutor executor)
            {
                var expectedNamespaces = ScriptExecutor.DefaultNamespaces;

                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                executor.ExecuteFile("script.csx");

                engine.Verify(i => i.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<IEnumerable<string>>(), It.Is<IEnumerable<string>>(x => !x.Except(expectedNamespaces).Any()), It.IsAny<ScriptPackSession>()), Times.Exactly(1));
            }

            [Theory, ScriptCsAutoData]
            public void ExecutorShouldPassDefaultReferencesToEngine([Frozen] Mock<IScriptEngine> engine, [Frozen] Mock<IFileSystem> fileSystem,
                                                                                                    [Frozen] Mock<IFilePreProcessor> preProcessor, ScriptExecutor executor)
            {
                var defaultReferences = ScriptExecutor.DefaultReferences;

                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns(new FilePreProcessorResult { Code = "var a = 0;" });

                executor.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                executor.ExecuteFile("script.csx");

                engine.Verify(i => i.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.Is<IEnumerable<string>>(x => !x.Except(defaultReferences).Any()), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Exactly(1));
            }
        }
    }
}