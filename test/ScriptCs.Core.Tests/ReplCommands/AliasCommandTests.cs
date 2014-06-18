using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class AliasCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsAlias()
            {
                // act
                var cmd = new AliasCommand(new Mock<IConsole>().Object);

                // assert
                Assert.Equal("alias", cmd.CommandName);
            }
        }
    }
}