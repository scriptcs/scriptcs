using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class ResetCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsReset()
            {
                var cmd = new ResetCommand();
                Assert.Equal("reset", cmd.CommandName);
            }
        }

        public class ExecuteMethod
        {
            [Fact]
            public void CallsReplReset()
            {
                var executor = new Mock<IScriptExecutor>();

                var cmd = new ResetCommand();
                var result = cmd.Execute(executor.Object, null);

                executor.Verify(x => x.Reset(), Times.Once);
            }
        }
    }
}