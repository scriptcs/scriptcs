using System;
using System.IO;
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
            public void ReturnsCd()
            {
                // act
                var cmd = new CdCommand();

                // assert
                Assert.Equal("cd", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void ChangesRelativePathBasedOnArg()
            {
                // arrange
                var fs = new Mock<IFileSystem>();
                var repl = new Mock<IRepl>();

                var tempPath = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

                fs.Setup(x => x.CurrentDirectory).Returns(Path.Combine(tempPath, "dir"));
                repl.Setup(x => x.FileSystem).Returns(fs.Object);

                var cmd = new CdCommand();

                // act
                cmd.Execute(repl.Object, new[] { ".." });

                // assert
                fs.VerifySet(x => x.CurrentDirectory = tempPath, Times.Once());
            }
        }
    }
}