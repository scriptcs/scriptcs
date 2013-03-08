using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptExecutorTests
    {
        private static ScriptExecutor CreateScriptExecutor(
            Mock<IFileSystem> fileSystem = null,
            Mock<IFilePreProcessor> fileProcessor = null,
            Mock<IScriptEngine> scriptEngine = null,
            Mock<IScriptHostFactory> scriptHostFactory = null)
        {
            fileSystem = fileSystem ?? new Mock<IFileSystem>();

            fileProcessor = fileProcessor ?? new Mock<IFilePreProcessor>();

            if (scriptEngine == null)
            {
                var mockSession = new Mock<ISession>();
                mockSession.Setup(s => s.AddReference(It.IsAny<string>()));
                mockSession.Setup(s => s.Execute(It.IsAny<string>())).Returns(new object());

                scriptEngine = new Mock<IScriptEngine>();
                scriptEngine.SetupProperty(e => e.BaseDirectory);
                scriptEngine.Setup(e => e.CreateSession()).Returns(mockSession.Object);
                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(mockSession.Object);
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
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor);
                executor.Execute(@"c:\my_script\script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                
                preProcessor.Verify(p => p.ProcessFile(@"c:\my_script\script.csx"));
            }

            [Fact]
            public void ShouldAddSystemAndSystemCoreReferencesToEngine()
            {
                // arrange
                var fileSystem = new Mock<IFileSystem>();
                var scriptEngine = new Mock<IScriptEngine>();
                var session = new Mock<ISession>();

                scriptEngine.Setup(e => e.AddReference("System")).Verifiable();
                scriptEngine.Setup(e => e.AddReference("System.Core")).Verifiable();
                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);

                var currentDirectory = @"C:\";
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                var scriptExecutor = CreateScriptExecutor(fileSystem: fileSystem, scriptEngine: scriptEngine);

                var scriptName = "script.csx";
                var paths = new string[0];
                IEnumerable<IScriptPack> recipes = Enumerable.Empty<IScriptPack>();

                // act
                scriptExecutor.Execute(scriptName, paths, recipes);

                // assert
                scriptEngine.Verify(e => e.AddReference("System"), Times.Once());
                scriptEngine.Verify(e => e.AddReference("System.Core"), Times.Once());
            }

            [Fact]
            public void ShouldSetEngineBaseDirectoryBasedOnCurrentDirectoryAndBinFolder()
            {
                // arrange
                var scriptEngine = new Mock<IScriptEngine>();
                var fileSystem = new Mock<IFileSystem>();
                var session = new Mock<ISession>();
                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);

                var currentDirectory = @"C:\";
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
                var scriptEngine = new Mock<IScriptEngine>();
                var fileSystem = new Mock<IFileSystem>();
                var session = new Mock<ISession>();

                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);

                var currentDirectory = @"C:\";
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                var binDirectory = Path.Combine(currentDirectory, "bin");

                fileSystem.Setup(fs => fs.DirectoryExists(binDirectory)).Returns(false).Verifiable();
                fileSystem.Setup(fs => fs.CreateDirectory(binDirectory)).Verifiable();

                var scriptExecutor = CreateScriptExecutor(fileSystem: fileSystem, scriptEngine: scriptEngine);

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
            public void ShouldExecuteScriptReturnedFromFileProcessorInSessionWhenExecuteIsInvoked()
            {
                // arrange
                var scriptEngine = new Mock<IScriptEngine>();
                var preProcessor = new Mock<IFilePreProcessor>();
                var fileSystem = new Mock<IFileSystem>();
                var session = new Mock<ISession>();

                string code = Guid.NewGuid().ToString();

                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);

                session.Setup(s => s.Execute(code)).Returns(null).Verifiable();

                var currentDirectory = @"C:\";
                fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

                var scriptExecutor = CreateScriptExecutor(
                    fileSystem: fileSystem, 
                    fileProcessor: preProcessor,
                    scriptEngine: scriptEngine);

                var scriptName = "script.csx";
                var paths = new string[0];
                IEnumerable<IScriptPack> recipes = Enumerable.Empty<IScriptPack>();

                preProcessor.Setup(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName))).Returns(code).Verifiable();

                // act
                scriptExecutor.Execute(scriptName, paths, recipes);

                // assert
                preProcessor.Verify(fs => fs.ProcessFile(Path.Combine(currentDirectory, scriptName)), Times.Once());
                session.Verify(s => s.Execute(code), Times.Once());
            }

            [Fact]
            public void ShouldInitializeScriptPacks()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor);

                var scriptPack1 = new Mock<IScriptPack>();
                scriptPack1.Setup(p => p.Initialize(It.IsAny<ISession>()));
                scriptPack1.Setup(p => p.GetContext()).Returns(Mock.Of<IScriptPackContext>());
                // var scriptPack2 = new Mock<IScriptPack>();
                // scriptPack2.Setup(p => p.Initialize(It.IsAny<ISession>()));

                executor.Execute("script.csx", Enumerable.Empty<string>(), new List<IScriptPack> { scriptPack1.Object });
                scriptPack1.Verify(p => p.Initialize(It.IsAny<ISession>()));
                // scriptPack2.Verify(p => p.Initialize(It.IsAny<ISession>()));
            }

            [Fact]
            public void ShouldCreateScriptHostWithContexts()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var scriptHostFactory = new Mock<IScriptHostFactory>();
                scriptHostFactory.Setup(f => f.CreateScriptHost(It.IsAny<IEnumerable<IScriptPackContext>>())).Returns((IEnumerable<IScriptPackContext> c) => new ScriptHost(c));

                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor, scriptHostFactory: scriptHostFactory);

                var scriptPack = new Mock<IScriptPack>();
                var context = new Mock<IScriptPackContext>().Object;

                scriptPack.Setup(p => p.GetContext()).Returns(context);

                executor.Execute("script.csx", Enumerable.Empty<string>(), new List<IScriptPack> { scriptPack.Object });
                scriptHostFactory.Verify(f => f.CreateScriptHost(It.IsAny<IEnumerable<IScriptPackContext>>()));
            }

            [Fact]
            public void ShouldCreateSessionWithScriptHost()
            {
                var fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var engine = new Mock<IScriptEngine>();
                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor, scriptEngine: engine);

                engine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(new Mock<ISession>().Object);
                executor.Execute("script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                engine.Verify(e=>e.CreateSession(It.IsAny<ScriptHost>()));
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
                var paths = new string[]{ sourceFilePath };

                var sourceWriteTime = new DateTime(2013, 3, 7);
                var destinatioWriteTime = new DateTime(2013, 3, 8);

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
                var session = new Mock<ISession>();

                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);

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

                session.Setup(e => e.AddReference(destinationFilePath1)).Verifiable();
                session.Setup(e => e.AddReference(destinationFilePath2)).Verifiable();
                session.Setup(e => e.AddReference(destinationFilePath3)).Verifiable();
                session.Setup(e => e.AddReference(destinationFilePath4)).Verifiable();

                // act
                scriptExecutor.Execute(scriptName, paths, Enumerable.Empty<IScriptPack>());

                // assert
                session.Verify(e => e.AddReference(destinationFilePath1), Times.Once());
                session.Verify(e => e.AddReference(destinationFilePath2), Times.Once());
                session.Verify(e => e.AddReference(destinationFilePath3), Times.Once());
                session.Verify(e => e.AddReference(destinationFilePath4), Times.Once());
            }
        }
    }
}