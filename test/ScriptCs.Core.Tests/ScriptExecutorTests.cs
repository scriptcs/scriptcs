using System;
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
            Mock<IScriptEngine> scriptEngine = null,
            Mock<IScriptHostFactory> scriptHostFactory = null)
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
                scriptEngine.SetupProperty(e => e.ScriptHostFactory);
            }

            if (scriptHostFactory == null)
            {
                return new ScriptExecutor(fileSystem.Object, fileProcessor.Object, scriptEngine.Object);
            }
            else
            {
                return new ScriptExecutor(fileSystem.Object, fileProcessor.Object, scriptEngine.Object, scriptHostFactory.Object);
            }
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
            public void ShouldCreateCurrentDirectoryIfItDoesNotExist()
            {
                // arrange
                var fileSystem = new Mock<IFileSystem>();

                var currentDirectory = @"C:\";
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                var binDirectory = Path.Combine(currentDirectory, "bin");

                fileSystem.Setup(fs => fs.DirectoryExists(binDirectory)).Returns(false).Verifiable();
                fileSystem.Setup(fs => fs.CreateDirectory(binDirectory)).Verifiable();

                var scriptExecutor = CreateScriptExecutor(fileSystem: fileSystem);

                var scriptName = "script.csx";
                var paths = new string[0];
                IEnumerable<IScriptPack> recipes = Enumerable.Empty<IScriptPack>();

                // act
                scriptExecutor.Execute(scriptName, paths, recipes);

                // assert
                fileSystem.Verify(fs => fs.DirectoryExists(binDirectory), Times.Once());
                fileSystem.Verify(fs => fs.CreateDirectory(binDirectory), Times.Once());
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
                IEnumerable<IScriptPack> recipes = Enumerable.Empty<IScriptPack>();

                preProcessor.Setup(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName))).Returns(code).Verifiable();
                scriptEngine.Setup(e => e.Execute(code, It.IsAny<IEnumerable<string>>(), recipes));

                // act
                scriptExecutor.Execute(scriptName, paths, recipes);

                // assert
                preProcessor.Verify(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName)), Times.Once());
   
                scriptEngine.Verify(s => s.Execute(code, It.IsAny<IEnumerable<string>>(), recipes), Times.Once());
 
            }

            [Fact]
            public void ShouldCopyFilesInPathIfLastWriteTimeDiffersFromLastWriteTimeOfFileInBin()
            {
                // arrange
                var fileSystem = new Mock<IFileSystem>();

                var scriptExecutor = CreateScriptExecutor(fileSystem: fileSystem);

                var currentDirectory = @"C:\";
                var sourceFilePath = Path.Combine(@"C:\fileDir", "fileName.cs");
                var destinationFilePath = Path.Combine(currentDirectory, @"bin\fileName.cs");

                var scriptName = "script.csx";
                var paths = new string[] { sourceFilePath };

                var sourceWriteTime = new DateTime(2013, 3, 7);
                var destinatioWriteTime = new DateTime(2013, 3, 8);

                fileSystem.Setup(fs => fs.GetWorkingDirectory(scriptName)).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.GetLastWriteTime(sourceFilePath)).Returns(sourceWriteTime).Verifiable();
                fileSystem.Setup(fs => fs.GetLastWriteTime(destinationFilePath)).Returns(destinatioWriteTime).Verifiable();
                fileSystem.Setup(fs => fs.Copy(sourceFilePath, destinationFilePath, true));

                // act
                scriptExecutor.Execute(scriptName, paths, Enumerable.Empty<IScriptPack>());

                // assert
                fileSystem.Verify(fs => fs.Copy(sourceFilePath, destinationFilePath, true), Times.Once());
                fileSystem.Verify(fs => fs.GetLastWriteTime(sourceFilePath), Times.Once());
                fileSystem.Verify(fs => fs.GetLastWriteTime(destinationFilePath), Times.Once());
            }

            [Fact]
            public void ShouldNotCopyFilesInPathIfLastWriteTimeEqualsLastWriteTimeOfFileInBin()
            {
                // arrange
                var fileSystem = new Mock<IFileSystem>();

                var scriptExecutor = CreateScriptExecutor(fileSystem: fileSystem);

                var currentDirectory = @"C:\";
                var sourceFilePath = Path.Combine(@"C:\fileDir", "fileName.cs");
                var destinationFilePath = Path.Combine(currentDirectory, @"bin\fileName.cs");

                var scriptName = "script.csx";
                var paths = new string[] { sourceFilePath };

                var sourceWriteTime = new DateTime(2013, 3, 7);
                var destinatioWriteTime = sourceWriteTime;

                fileSystem.Setup(fs => fs.GetWorkingDirectory(scriptName)).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.GetLastWriteTime(sourceFilePath)).Returns(sourceWriteTime).Verifiable();
                fileSystem.Setup(fs => fs.GetLastWriteTime(destinationFilePath)).Returns(destinatioWriteTime).Verifiable();
                fileSystem.Setup(fs => fs.Copy(sourceFilePath, destinationFilePath, true));

                // act
                scriptExecutor.Execute(scriptName, paths, Enumerable.Empty<IScriptPack>());

                // assert
                fileSystem.Verify(fs => fs.Copy(sourceFilePath, destinationFilePath, true), Times.Never());
                fileSystem.Verify(fs => fs.GetLastWriteTime(sourceFilePath), Times.Once());
                fileSystem.Verify(fs => fs.GetLastWriteTime(destinationFilePath), Times.Once());
            }

            [Fact]
            public void ShouldAddReferenceToEachDestinationFile()
            {
                // arrange
                var fileSystem = new Mock<IFileSystem>();
                var scriptEngine = new Mock<IScriptEngine>();

                var scriptExecutor = CreateScriptExecutor(fileSystem: fileSystem, scriptEngine: scriptEngine);

                var currentDirectory = @"C:\";
                var sourceFilePath1 = Path.Combine(@"C:\fileDir", "fileName1.cs");
                var sourceFilePath2 = Path.Combine(@"C:\fileDir", "fileName2.cs");
                var sourceFilePath3 = Path.Combine(@"C:\fileDir", "fileName3.cs");
                var sourceFilePath4 = Path.Combine(@"C:\fileDir", "fileName4.cs");
                var destinationFilePath1 = Path.Combine(currentDirectory, @"bin\fileName1.cs");
                var destinationFilePath2 = Path.Combine(currentDirectory, @"bin\fileName2.cs");
                var destinationFilePath3 = Path.Combine(currentDirectory, @"bin\fileName3.cs");
                var destinationFilePath4 = Path.Combine(currentDirectory, @"bin\fileName4.cs");

                var scriptName = "script.csx";

                var paths = new string[] { sourceFilePath1, sourceFilePath2, sourceFilePath3, sourceFilePath4 };

                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);
                fileSystem.Setup(fs => fs.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);

                var destPaths = new string[] { "System", "System.Core", destinationFilePath1, destinationFilePath2, destinationFilePath3, destinationFilePath4 };

                scriptEngine.Setup(e => e.Execute(It.IsAny<string>(), It.Is<IEnumerable<string>>(x => x.SequenceEqual(destPaths)), It.IsAny<IEnumerable<IScriptPack>>()));

                // act
                scriptExecutor.Execute(scriptName, paths, Enumerable.Empty<IScriptPack>());

                // assert
                scriptEngine.Verify(e => e.Execute(It.IsAny<string>(), It.Is<IEnumerable<string>>(x => x.SequenceEqual(destPaths)), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
            }
        }
    }
}