using System;
using System.IO;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    using ScriptCs.Exceptions;

    public class DebugScriptExecutorTests
    {
        public static DebugScriptExecutor CreateScriptExecutor(
            Mock<IFileSystem> fileSystem = null,
            Mock<IFilePreProcessor> fileProcessor = null,
            Mock<IScriptEngine> scriptEngine = null,
            Mock<IScriptHostFactory> scriptHostFactory = null,
            Mock<ICompiledDllDebugger> compiledDllDebugger = null)
        {
            if (fileSystem == null)
            {
                fileSystem = new Mock<IFileSystem>();
                fileSystem.Setup(fs => fs.GetWorkingDirectory(It.IsAny<string>())).Returns(@"C:\");
                fileSystem.Setup(fs => fs.CreateFileStream(It.IsAny<string>(), It.IsAny<FileMode>()))
                          .Returns(new MemoryStream());
            }

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

            compiledDllDebugger = compiledDllDebugger ?? new Mock<ICompiledDllDebugger>();

            if (scriptHostFactory == null)
            {
                return new DebugScriptExecutor(fileSystem.Object, fileProcessor.Object, scriptEngine.Object, compiledDllDebugger.Object);
            }
            else
            {
                return new DebugScriptExecutor(fileSystem.Object, fileProcessor.Object, scriptEngine.Object, compiledDllDebugger.Object, scriptHostFactory.Object);
            }
        }

        public class TheExecuteMethod
        {
            [Fact]
            public void ShouldCompileProcessedCode()
            {
                // arrange
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var scriptEngine = new Mock<IScriptEngine>();
                var session = new Mock<ISession>();
                var submission = new Mock<ISubmission<object>>();
                var compilation = new Mock<ICompilation>();
                var compilationResult = new Mock<ICompilationResult>();

                const string PathToScript = @"C:\script.csx";
                var code = Guid.NewGuid().ToString();

                compilationResult.Setup(r => r.Success).Returns(true);

                filePreProcessor.Setup(p => p.ProcessFile(PathToScript)).Returns(code);

                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);
                scriptEngine.SetupProperty(e => e.BaseDirectory);

                session.Setup(s => s.CompileSubmission<object>(code)).Returns(submission.Object).Verifiable();
                session.Setup(s => s.Engine).Returns(scriptEngine.Object);

                submission.Setup(s => s.Compilation).Returns(compilation.Object);

                compilation.Setup(c => c.Emit(It.IsAny<Stream>(), It.IsAny<Stream>())).Returns(compilationResult.Object).Verifiable();

                var scriptExecutor = DebugScriptExecutorTests.CreateScriptExecutor(
                    scriptEngine: scriptEngine,
                    fileProcessor: filePreProcessor);

                // act
                scriptExecutor.Execute(PathToScript, Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // assert
                session.Verify(s => s.CompileSubmission<object>(code), Times.Once());
            }

            [Fact]
            public void ShouldEmitCompilationProvidingPathsForDllAndPdbFiles()
            {
                // arrange
                var scriptEngine = new Mock<IScriptEngine>();
                var session = new Mock<ISession>();
                var submission = new Mock<ISubmission<object>>();
                var compilation = new Mock<ICompilation>();
                var compilationResult = new Mock<ICompilationResult>();

                var fileSystem = new Mock<IFileSystem>();

                const string PathToScript = @"C:\script.csx";
                const string BinDir = @"C:\bin";
                const string OutputDllName = "script.dll";
                const string OutputPdbName = "script.pdb";
                var pdbFullPath = Path.Combine(BinDir, OutputPdbName);
                var dllFullPath = Path.Combine(BinDir, OutputDllName);

                var pdbStream = new MemoryStream();
                var dllStream = new MemoryStream();

                compilationResult.Setup(r => r.Success).Returns(true);

                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);
                scriptEngine.SetupProperty(e => e.BaseDirectory);

                session.Setup(s => s.CompileSubmission<object>(It.IsAny<string>())).Returns(submission.Object);
                session.Setup(s => s.Engine).Returns(scriptEngine.Object);

                submission.Setup(s => s.Compilation).Returns(compilation.Object);

                fileSystem.Setup(fs => fs.GetWorkingDirectory(PathToScript)).Returns(@"C:\");
                fileSystem.Setup(fs => fs.CreateFileStream(pdbFullPath, FileMode.OpenOrCreate)).Returns(pdbStream).Verifiable();
                fileSystem.Setup(fs => fs.CreateFileStream(dllFullPath, FileMode.OpenOrCreate)).Returns(dllStream).Verifiable();

                compilation.Setup(c => c.Emit(dllStream, pdbStream)).Returns(compilationResult.Object).Verifiable();

                var scriptExecutor = DebugScriptExecutorTests.CreateScriptExecutor(fileSystem, scriptEngine: scriptEngine);

                // act
                scriptExecutor.Execute(PathToScript, Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // assert
                compilation.Verify(c => c.Emit(dllStream, pdbStream), Times.Once());
            }

            [Fact]
            public void ShouldThrowCompilationExceptionIfCompilationFails()
            {
                // arrange
                var scriptEngine = new Mock<IScriptEngine>();
                var session = new Mock<ISession>();
                var submission = new Mock<ISubmission<object>>();
                var compilation = new Mock<ICompilation>();
                var compilationResult = new Mock<ICompilationResult>();

                const string ErrorMessage = "Error message";
                compilationResult.Setup(r => r.Success).Returns(false).Verifiable();
                compilationResult.Setup(r => r.ErrorMessage).Returns(ErrorMessage).Verifiable();

                const string PathToScript = @"C:\script.csx";

                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);
                scriptEngine.SetupProperty(e => e.BaseDirectory);

                session.Setup(s => s.CompileSubmission<object>(It.IsAny<string>())).Returns(submission.Object);
                session.Setup(s => s.Engine).Returns(scriptEngine.Object);

                submission.Setup(s => s.Compilation).Returns(compilation.Object);

                compilation.Setup(c => c.Emit(It.IsAny<Stream>(), It.IsAny<Stream>())).Returns(compilationResult.Object).Verifiable();

                var scriptExecutor = DebugScriptExecutorTests.CreateScriptExecutor(scriptEngine: scriptEngine);

                // act + assert
                var exception = Assert.Throws<CompilationException>(() =>
                    scriptExecutor.Execute(PathToScript, Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>()));

                exception.Message.ShouldEqual(ErrorMessage);

                compilationResult.Verify(r => r.ErrorMessage, Times.Once());
                compilationResult.Verify(r => r.Success, Times.Once());
            }

            [Fact]
            public void ShouldRunCompileAssemblyRunnerOnOutputPathIfCompilationSucceeds()
            {
                // arrange
                var scriptEngine = new Mock<IScriptEngine>();
                var session = new Mock<ISession>();
                var submission = new Mock<ISubmission<object>>();
                var compilation = new Mock<ICompilation>();
                var compilationResult = new Mock<ICompilationResult>();
                var compiledDllDebugger = new Mock<ICompiledDllDebugger>();

                compilationResult.Setup(r => r.Success).Returns(true).Verifiable();

                const string PathToScript = @"C:\script.csx";
                const string BinDir = @"C:\bin";
                const string OutputDllName = "script.dll";
                var dllFullPath = Path.Combine(BinDir, OutputDllName);

                scriptEngine.Setup(e => e.CreateSession(It.IsAny<ScriptHost>())).Returns(session.Object);
                scriptEngine.SetupProperty(e => e.BaseDirectory);

                session.Setup(s => s.CompileSubmission<object>(It.IsAny<string>())).Returns(submission.Object);
                session.Setup(s => s.Engine).Returns(scriptEngine.Object);

                submission.Setup(s => s.Compilation).Returns(compilation.Object);

                compilation.Setup(c => c.Emit(It.IsAny<Stream>(), It.IsAny<Stream>())).Returns(compilationResult.Object).Verifiable();

                compiledDllDebugger.Setup(r => r.Run(dllFullPath, session.Object)).Verifiable();

                var scriptExecutor = DebugScriptExecutorTests.CreateScriptExecutor(scriptEngine: scriptEngine, compiledDllDebugger: compiledDllDebugger);

                // act
                scriptExecutor.Execute(PathToScript, Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());

                // assert
                compilationResult.Verify(r => r.Success, Times.Once());
                compiledDllDebugger.Verify(r => r.Run(dllFullPath, session.Object), Times.Once());
            }
        }
    }
}
