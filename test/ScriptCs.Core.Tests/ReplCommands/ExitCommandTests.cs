﻿using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class ExitCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsExit()
            {
                // act
                var cmd = new ExitCommand(new Mock<IConsole>().Object);

                // assert
                Assert.Equal("exit", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void PromptsUserBeforeExiting()
            {
                // arrange
                const string message = "Are you sure you wish to exit? (y/n): ";
                var console = new Mock<IConsole>();
                console.Setup(x => x.ReadLine()).Returns("n");
                var executor = new Mock<IRepl>();
                var cmd = new ExitCommand(console.Object);

                // act
                cmd.Execute(executor.Object, null);

                // assert
                console.Verify(x => x.Write(message));
            }

            [Fact]
            public void ExitsWhenUserAnswersYes()
            {
                // arrange
                var console = new Mock<IConsole>();
                console.Setup(x => x.ReadLine()).Returns("y");

                var executor = new Mock<IRepl>();
                var cmd = new ExitCommand(console.Object);

                // act
                cmd.Execute(executor.Object, null);

                // assert
                executor.Verify(x => x.Quit());
            }

            [Fact]
            public void DoesNotExitWhenUserAnswersNo()
            {
                // arrange
                var console = new Mock<IConsole>();
                console.Setup(x => x.ReadLine()).Returns("n");

                var executor = new Mock<IRepl>();
                var cmd = new ExitCommand(console.Object);

                // act
                cmd.Execute(executor.Object, null);

                // assert
                executor.Verify(x => x.Quit(), Times.Never);
            }
        }
    }
}
