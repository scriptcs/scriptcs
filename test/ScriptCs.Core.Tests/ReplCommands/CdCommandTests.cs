using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class CdCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsCwd()
            {
                var cmd = new CdCommand();
                Assert.Equal("cd", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void ChangesRelativePathBasedOnArg()
            {
                var fs = new Mock<IFileSystem>();
                var executor = new Mock<IScriptExecutor>();

                fs.Setup(x => x.CurrentDirectory).Returns(@"c:\dir");

                executor.Setup(x => x.FileSystem).Returns(fs.Object);

                var cmd = new CdCommand();
                var result = cmd.Execute(executor.Object, new [] {".."});


                fs.VerifySet(x => x.CurrentDirectory = @"c:\", Times.Once());
            }
        }
    }
}