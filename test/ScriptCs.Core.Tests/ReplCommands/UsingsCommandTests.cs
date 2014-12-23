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
            private Mock<IRepl> _repl;

            public ExecuteMethod()
            {
                _repl = new Mock<IRepl>();
            }

             [Fact]
            public void ShouldReturnNamespacesFromExecutor()
            {
                var ns = new List<string> {"System"};
                _repl.SetupGet(x => x.Namespaces).Returns(ns);

                var cmd = new UsingsCommand();
                var result = cmd.Execute(_repl.Object, null);

                result.ShouldBeSameAs(ns);
            }
        }
    }
}