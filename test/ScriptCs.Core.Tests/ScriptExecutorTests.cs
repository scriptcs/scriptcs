using System.Linq;
using Moq;
using ScriptCs.Contracts;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptExecutorTests
    {
        private readonly Mock<IFileSystem> _fileSystem;
        private readonly Mock<IFilePreProcessor> _preProcessor;

        public ScriptExecutorTests()
        {
            _fileSystem = new Mock<IFileSystem>();
            _fileSystem.Setup(f => f.CurrentDirectory).Returns(@"c:\my_script");
            _fileSystem.Setup(f => f.GetWorkingDirectory(It.IsAny<string>())).Returns(@"c:\my_script");

            _preProcessor = new Mock<IFilePreProcessor>();
            _preProcessor.Setup(p => p.ProcessFile(It.IsAny<string>())).Returns("var a = 0;");
        }

        [Fact]
        public void ConstructsAbsolutePathBeforePreProcessingFile()
        {            
            var executor = new ScriptExecutor(_fileSystem.Object, _preProcessor.Object);
            executor.Execute("script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptCsRecipe>());
            _preProcessor.Verify(p => p.ProcessFile(@"c:\my_script\script.csx"));
        }

        [Fact]
        public void DoNotChangePathIfAbsolute()
        {
            var executor = new ScriptExecutor(_fileSystem.Object, _preProcessor.Object);
            executor.Execute(@"c:\my_script\script.csx", Enumerable.Empty<string>(), Enumerable.Empty<IScriptCsRecipe>());
            _preProcessor.Verify(p => p.ProcessFile(@"c:\my_script\script.csx"));
        }
    }
}