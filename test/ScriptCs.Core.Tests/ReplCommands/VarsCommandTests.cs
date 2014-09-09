using System.Collections.Generic;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Should;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class VarsCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsUsings()
            {
                // act
                var cmd = new VarsCommand();

                // assert
                cmd.CommandName.ShouldEqual("vars");
            }
        }

        public class ExecuteMethod
        {
            private Mock<IScriptExecutor> _executor;

            public ExecuteMethod()
            {
                _executor = new Mock<IScriptExecutor>();
            }

            [Fact]
            public void ShouldReturnLocalVarsFromEngine()
            {
                var locals = new List<string> {"int x = 0"};
                var replEngine = new Mock<IReplEngine>();
                replEngine.SetupGet(x => x.LocalVariables).Returns(locals);
                _executor.SetupGet(x => x.ScriptEngine).Returns(replEngine.Object);

                var cmd = new VarsCommand();
                var result = cmd.Execute(_executor.Object, null);

                result.ShouldBeSameAs(locals);
            }

            [Fact]
            public void ShouldReturnNullForEngineWhichisNotReplEngine()
            {
                var replEngine = new Mock<IScriptEngine>();
                _executor.SetupGet(x => x.ScriptEngine).Returns(replEngine.Object);

                var cmd = new VarsCommand();
                var result = cmd.Execute(_executor.Object, null);

                result.ShouldBeNull();
            }
        }
    }
}