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
    // had to update this class as unit tests did not follow approach requested in contrib (phil hack lipowerke tests)
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
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
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
                var scriptEngine = new Mock<IScriptEngine>();
                var fileSystem = new Mock<IFileSystem>();
                var session = new Mock<ISession>();

                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);

                var currentDirectory = @"C:\";
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(currentDirectory);
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
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

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
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

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
                fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

                var preProcessor = new Mock<IFilePreProcessor>();
                preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");

                var engine = new Mock<IScriptEngine>();
                var executor = CreateScriptExecutor(fileSystem: fileSystem, fileProcessor: preProcessor, scriptEngine: engine);

                engine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(new Mock<ISession>().Object);
                executor.Execute("script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                engine.Verify(e=>e.CreateSession(It.IsAny<ScriptHost>()));
            } 

        }
    }
}