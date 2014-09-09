using System.Collections.Generic;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Should;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class UsingsCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsUsings()
            {
                // act
                var cmd = new UsingsCommand();

                // assert
                cmd.CommandName.ShouldEqual("usings");
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
            public void ShouldReturnNamespacesFromExecutor()
            {
                var ns = new List<string> {"System"};
                _executor.SetupGet(x => x.Namespaces).Returns(ns);

                var cmd = new UsingsCommand();
                var result = cmd.Execute(_executor.Object, null);

                result.ShouldBeSameAs(ns);
            }
        }
    }
}