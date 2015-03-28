using System.Collections.Generic;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class ScriptPacksCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsScriptPacks()
            {
                // act
                var cmd = new ScriptPacksCommand(new Mock<IConsole>().Object);

                // assert
                Assert.Equal("scriptpacks", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void ShouldExitIfThereAreNoScriptPacks()
            {
                // arrange
                var console = new Mock<IConsole>();
                var repl = new Mock<IRepl>();
                var scriptPackSession = new Mock<ScriptPackSession>(Enumerable.Empty<IScriptPack>(), new string[0]);

                scriptPackSession.Setup(x => x.Contexts).Returns((IEnumerable<IScriptPackContext>) null);
                repl.Setup(x => x.ScriptPackSession).Returns(scriptPackSession.Object);

                var cmd = new ScriptPacksCommand(console.Object);

                // act
                cmd.Execute(repl.Object, null);

                // assert
                console.Verify(x => x.WriteLine("There are no script packs available in this REPL session"));
            }
        }
    }
}