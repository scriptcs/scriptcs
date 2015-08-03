using System.Collections.Generic;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class HelpCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsHelp()
            {
                // act
                var cmd = new HelpCommand(new Mock<IConsole>().Object);

                // assert
                Assert.Equal("help", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void PrintsCommandsToConsole()
            {
                // arrange
                var console = new Mock<IConsole>();
                var repl = new Mock<IRepl>();
                var clearCommand = new ClearCommand(console.Object);
                var exitCommand = new ExitCommand(console.Object);

                var commands = new Dictionary<string, IReplCommand>
                {
                    {clearCommand.CommandName, clearCommand},
                    {exitCommand.CommandName, exitCommand},
                };

                repl.Setup(x => x.Commands).Returns(commands);

                var cmd = new HelpCommand(console.Object);

                // act
                cmd.Execute(repl.Object, null);

                // assert
                console.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Exactly(3));
                console.Verify(x => x.WriteLine(It.Is<string>(f => f.StartsWith(":" + clearCommand.CommandName) && f.Contains(clearCommand.Description))), Times.Once);
                console.Verify(x => x.WriteLine(It.Is<string>(f => f.StartsWith(":" + exitCommand.CommandName) && f.Contains(exitCommand.Description))), Times.Once);
            }

            [Fact]
            public void CorrectlyPrintsCommandsToConsoleAfterAlias()
            {
                // arrange
                var console = new Mock<IConsole>();
                var repl = new Mock<IRepl>();
                var clearCommand = new ClearCommand(console.Object);
                var aliasCommand = new AliasCommand(console.Object);

                var commands = new Dictionary<string, IReplCommand>
                {
                    {clearCommand.CommandName, clearCommand},
                    {aliasCommand.CommandName, aliasCommand},
                };

                repl.Setup(x => x.Commands).Returns(commands);
                var cmd = new HelpCommand(console.Object);

                aliasCommand.Execute(repl.Object, new[] { "clear", "clr" });

                // act
                cmd.Execute(repl.Object, null);

                // assert
                console.Verify(x => x.WriteLine(It.Is<string>(f => f.StartsWith(":" + clearCommand.CommandName) && f.Contains(clearCommand.Description))), Times.Once);
                console.Verify(x => x.WriteLine(It.Is<string>(f => f.StartsWith(":" + aliasCommand.CommandName) && f.Contains(aliasCommand.Description))), Times.Once);
                console.Verify(x => x.WriteLine(It.Is<string>(f => f.StartsWith(":clr") && f.Contains(clearCommand.Description))), Times.Once);
            }
        }
    }
}