using System.Collections;
using System.Collections.Generic;
using System.Text;
using Moq;

using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;

using Should;

using Xunit.Extensions;
using System;

namespace ScriptCs.Tests
{
    public class ExecuteReplCommandTests
    {
        public class TheExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldPromptForInput(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IInputLine> inputLine,
                CommandFactory factory)
            {
                // Arrange
                var readLines = 0;
                var builder = new StringBuilder();
                var args = new ScriptCsArgs { Repl = true };

                fileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");

                inputLine.Setup(x => x.ReadLine()).Callback(() => readLines++).Throws(new Exception());
                console.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(value => builder.Append(value));

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                builder.ToString().EndsWith("> ").ShouldBeTrue();
                readLines.ShouldEqual(1);
            }

            [Theory, ScriptCsAutoData]
            public void WhenPassedAScript_ShouldPressedReplWithScript(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IInputLine> inputLine, 
                CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Repl = true, ScriptName = "test.csx" };

                inputLine.Setup(x => x.ReadLine()).Returns(() =>
                {
                    inputLine.Setup(x => x.ReadLine()).Throws(new Exception());
                    return string.Empty;
                });
                fileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");
                scriptEngine.Setup(
                    x => x.Execute("#load test.csx", It.IsAny<string[]>(), It.IsAny<AssemblyReferences>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()));

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                scriptEngine.Verify();
            }

            [Theory, ScriptCsAutoData]
            public void WhenNotPassedAScript_ShouldNotCallTheEngineAutomatically(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IInputLine> inputLine, 
                CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { Repl = true };

                inputLine.Setup(x => x.ReadLine()).Returns(() =>
                {
                    inputLine.Setup(x => x.ReadLine()).Throws(new Exception());
                    return string.Empty;
                });
                fileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                scriptEngine.Verify(
                    x => x.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<AssemblyReferences>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Never());
            }
        }
    }
}
