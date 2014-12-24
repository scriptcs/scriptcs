using System;
using System.IO;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class CwdCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsCwd()
            {
                // act
                var cmd = new CwdCommand(new Mock<IConsole>().Object);

                // assert
                Assert.Equal("cwd", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void PrintsCurrentWorkingDirectoryToConsole()
            {
                // arrange
                var console = new Mock<IConsole>();
                var fs = new Mock<IFileSystem>();
                var repl = new Mock<IRepl>();

                var tempPath = Path.GetTempPath();

                fs.Setup(x => x.CurrentDirectory).Returns(tempPath);
                repl.Setup(x => x.FileSystem).Returns(fs.Object);

                var cmd = new CwdCommand(console.Object);

                // act
                cmd.Execute(repl.Object, null);

                // assert
                console.Verify(x => x.WriteLine(tempPath));
            }

            [Fact]
            public void PreservesConsoleColors()
            {
                // arrange
                var console = new Mock<IConsole>();
                var repl = new Mock<IRepl>();

                console.SetupProperty(x => x.ForegroundColor);
                repl.Setup(x => x.FileSystem).Returns(new Mock<IFileSystem>().Object);

                var cmd = new CwdCommand(console.Object);
                var expectedForegroundColor = console.Object.ForegroundColor;

                // act
                cmd.Execute(repl.Object, null);

                // assert
                Assert.Equal(expectedForegroundColor, console.Object.ForegroundColor);
            }
        }
    }
}