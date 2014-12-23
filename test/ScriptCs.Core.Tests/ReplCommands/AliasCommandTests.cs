using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Should;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class AliasCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ShouldReturnAlias()
            {
                // act
                var cmd = new AliasCommand(new Mock<IConsole>().Object);

                // assert
                cmd.CommandName.ShouldEqual("alias");
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void ShouldAliasCommandWithNewName()
            {
                // arrange
                var currentDir = @"C:\";
                var dummyCommand = new Mock<IReplCommand>();
                dummyCommand.Setup(x => x.CommandName).Returns("foo");

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.BinFolder).Returns(Path.Combine(currentDir, "bin"));
                fs.Setup(x => x.DllCacheFolder).Returns(Path.Combine(currentDir, "cache"));

                var console = new Mock<IConsole>();
                var executor = new Repl(null, fs.Object, null, null, null, null, null, new List<IReplCommand> { dummyCommand.Object });

                var cmd = new AliasCommand(console.Object);

                // act
                cmd.Execute(executor, new[] { "foo", "bar" });

                // assert
                executor.Commands.Count.ShouldEqual(2);
                executor.Commands["bar"].ShouldBeSameAs(executor.Commands["foo"]);
            }
        }
    }
}