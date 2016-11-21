using System.Collections.Generic;
using System.IO;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Should;
using Xunit;
using Xunit.Extensions;

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
            [Theory, ScriptCsAutoData]
            public void ShouldAliasCommandWithNewName(
                Mock<IFileSystem> fileSystem,
                Mock<IScriptEngine> engine,
                Mock<IObjectSerializer> serializer,
                TestLogProvider logProvider,
                Mock<IScriptLibraryComposer> composer,
                Mock<IConsole> console,
                Mock<IFilePreProcessor> filePreProcessor)
            {
                // arrange
                var currentDir = @"C:\";
                var dummyCommand = new Mock<IReplCommand>();
                dummyCommand.Setup(x => x.CommandName).Returns("foo");

                fileSystem.Setup(x => x.BinFolder).Returns(Path.Combine(currentDir, "bin"));
                fileSystem.Setup(x => x.DllCacheFolder).Returns(Path.Combine(currentDir, "cache"));

                var executor = new Repl(
                    new string[0],
                    fileSystem.Object,
                    engine.Object,
                    serializer.Object,
                    logProvider,
                    composer.Object,
                    console.Object,
                    filePreProcessor.Object,
                    new List<IReplCommand> { dummyCommand.Object },
                    new Printers(serializer.Object));

                var cmd = new AliasCommand(console.Object);

                // act
                cmd.Execute(executor, new[] { "foo", "bar" });

                // assert
                executor.Commands.Count.ShouldEqual(2);
                executor.Commands["bar"].ShouldBeSameAs(executor.Commands["foo"]);
            }

            [Fact]
            public void ShouldNotThrowAnExceptionWhenAnUnknownCommandIsPassed()
            {
                // arrange
                var command = new AliasCommand(new Mock<IConsole>().Object);

                // act
                var exception = Record.Exception(() => command.Execute(new Mock<IRepl>().Object, null));

                // assert
                exception.ShouldBeNull();
            }
        }
    }
}
