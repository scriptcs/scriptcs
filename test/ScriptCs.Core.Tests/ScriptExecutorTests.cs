﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptExecutorTests
    {
        public static ScriptExecutor CreateScriptExecutor(
            Mock<IFileSystem> fileSystem = null,
            Mock<IFilePreProcessor> fileProcessor = null,
            Mock<IScriptEngine> scriptEngine = null)
        {
            if (fileSystem == null)
            {
                fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(fs => fs.GetWorkingDirectory(It.IsAny<string>())).Returns(@"C:\");
            }

            fileProcessor = fileProcessor ?? new Mock<IFilePreProcessor>();

            if (scriptEngine == null)
            {
                scriptEngine = new Mock<IScriptEngine>();
                scriptEngine.SetupProperty(e => e.BaseDirectory);
            }

            return new ScriptExecutor(fileSystem.Object, fileProcessor.Object, scriptEngine.Object);
        }

        public class TheExecuteMethod
        {
            [Fact]
            public void ConstructsAbsolutePathBeforePreProcessingFile()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor);

                executor.Execute("script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                preProcessor.Verify(p => p.ProcessFile(@"c:\my_script\script.csx"));
            }

            [Fact]
            public void DoNotChangePathIfAbsolute()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor);
                executor.Execute(@"c:\my_script\script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                preProcessor.Verify(p => p.ProcessFile(@"c:\my_script\script.csx"));
            }

            [Fact]
            public void ShouldSetEngineBaseDirectoryBasedOnCurrentDirectoryAndBinFolder()
            {
                // arrange
                var scriptEngine = new Mock<IScriptEngine>();
                var fileSystem = new Mock<IFileSystem>();

                var currentDirectory = @"C:\";
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                scriptEngine.SetupProperty(e => e.BaseDirectory);

                var scriptExecutor = CreateScriptExecutor(fileSystem: fileSystem, scriptEngine: scriptEngine);

                var scriptName = "script.csx";
                var paths = new string[0];
                IEnumerable<IScriptPack> recipes = Enumerable.Empty<IScriptPack>();

                // act
                scriptExecutor.Execute(scriptName, paths, recipes);

                // assert
                string expectedBaseDirectory = Path.Combine(currentDirectory, "bin");
                expectedBaseDirectory.ShouldEqual(scriptEngine.Object.BaseDirectory);
            }

            [Fact]
            public void ShouldExecuteScriptReturnedFromFileProcessorInScriptEngineWhenExecuteIsInvoked()
            {
                // arrange
                var scriptEngine = new Mock<IScriptEngine>();
                var preProcessor = new Mock<IFilePreProcessor>();
                var fileSystem = new Mock<IFileSystem>();

                string code = Guid.NewGuid().ToString();

                var currentDirectory = @"C:\";
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                var scriptExecutor = CreateScriptExecutor(
                    fileSystem: fileSystem,
                    fileProcessor: preProcessor,
                    scriptEngine: scriptEngine);

                var scriptName = "script.csx";
                var paths = new string[0];
                var recipes = Enumerable.Empty<IScriptPack>();

                preProcessor.Setup(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName))).Returns(code).Verifiable();
                scriptEngine.Setup(e => e.Execute(code, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()));

                // act
                scriptExecutor.Execute(scriptName, paths, recipes);

                // assert
                preProcessor.Verify(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName)), Times.Once());

                scriptEngine.Verify(s => s.Execute(code, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Once());
 
            }

            [Fact]
            public void ShouldAddReferenceToEachDestinationFile()
            {
                // arrange
                var defaultReferences = new[] {"System", "System.Core", "System.Data", "System.Data.DataSetExtensions", "System.Xml", "System.Xml.Linq"};
                var fileSystem = new Mock<IFileSystem>();
                var scriptEngine = new Mock<IScriptEngine>();

                var scriptExecutor = CreateScriptExecutor(fileSystem: fileSystem, scriptEngine: scriptEngine);

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

                scriptEngine.Setup(e => e.Execute(It.IsAny<string>(), It.Is<IEnumerable<string>>(x => x.SequenceEqual(defaultReferences.Union(destPaths))), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()));

                // act
                scriptExecutor.Execute(scriptName, paths, Enumerable.Empty<IScriptPack>());

                // assert
                scriptEngine.Verify(e => e.Execute(It.IsAny<string>(), It.Is<IEnumerable<string>>(x => x.SequenceEqual(defaultReferences.Union(destPaths))), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Once());
            }

            [Fact]
            public void ShouldInitializeScriptPacks()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor);

                var scriptPack1 = new Mock<IScriptPack>();
                scriptPack1.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());

                executor.Execute("script.csx", Enumerable.Empty<string>(), new[] { scriptPack1.Object });

                scriptPack1.Verify(p => p.Initialize(It.IsAny<IScriptPackSession>()));
            }

            [Fact]
            public void ShouldTerminateScriptPacksWhenScriptFinishes()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor);

                var scriptPack1 = new Mock<IScriptPack>();
                scriptPack1.Setup(p => p.Initialize(It.IsAny<IScriptPackSession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());
                scriptPack1.Setup(p => p.Terminate());

                executor.Execute("script.csx", Enumerable.Empty<string>(), new[] { scriptPack1.Object });

                scriptPack1.Verify(p => p.Terminate());
            }

            [Fact]
            public void ExecutorShouldPassDefaultNamespacesToEngine()
            {
                var expectedNamespaces = new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks"};

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var engine = new Mock<IScriptEngine>();

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor, scriptEngine:engine);

                executor.Execute("script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                engine.Verify(i => i.Execute(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.Is<IEnumerable<string>>(x => !x.Except(expectedNamespaces).Any()), It.IsAny<ScriptPackSession>()), Times.Exactly(1));
            }

            [Fact]
            public void ExecutorShouldPassDefaultReferencesToEngine()
            {
                var defaultReferences = new[] { "System", "System.Core", "System.Data", "System.Data.DataSetExtensions", "System.Xml", "System.Xml.Linq" };

                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var engine = new Mock<IScriptEngine>();

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor, scriptEngine: engine);

                executor.Execute("script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                engine.Verify(i => i.Execute(It.IsAny<string>(), It.Is<IEnumerable<string>>(x => !x.Except(defaultReferences).Any()), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Exactly(1));
            }
        }
    }
}