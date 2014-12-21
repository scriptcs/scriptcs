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
            private Mock<IRepl> _repl;

            public ExecuteMethod()
            {
                _repl = new Mock<IRepl>();
            }

            [Fact]
            public void ShouldReturnLocalVarsFromEngine()
            {
                var locals = new List<string> {"int x = 0"};
                var replEngine = new Mock<IReplEngine>();
                replEngine.Setup(x => x.GetLocalVariables(It.IsAny<ScriptPackSession>())).Returns(locals);
                _repl.SetupGet(x => x.ScriptEngine).Returns(replEngine.Object);

                var cmd = new VarsCommand();
                var result = cmd.Execute(_repl.Object, null);

                result.ShouldBeSameAs(locals);
            }

            [Fact]
            public void ShouldReturnNullForEngineWhichisNotReplEngine()
            {
                var replEngine = new Mock<IScriptEngine>();
                _repl.SetupGet(x => x.ScriptEngine).Returns(replEngine.Object);

                var cmd = new VarsCommand();
                var result = cmd.Execute(_repl.Object, null);

                result.ShouldBeNull();
            }
        }
    }
}