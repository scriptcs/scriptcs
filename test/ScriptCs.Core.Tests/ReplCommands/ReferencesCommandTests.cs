using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.ReplCommands;
using Should;
using Xunit;

namespace ScriptCs.Tests.ReplCommands
{
    public class ReferencesCommandTests
    {
        public class CommandNameProperty
        {
            [Fact]
            public void ReturnsReferences()
            {
                // act
                var cmd = new ReferencesCommand();

                // assert
                cmd.CommandName.ShouldEqual("references");
            }
        }

        public class ExecuteMethod
        {
            private readonly Mock<IRepl> _repl;

            public ExecuteMethod()
            {
                _repl = new Mock<IRepl>();
            }

            [Fact]
            public void ShouldReturnAssembliesFromExecutor()
            {
                var assemblies = new AssemblyReferences(
                    new List<Assembly> { typeof(string).Assembly }, new List<string> { "path1", "path2" });

                _repl.SetupGet(x => x.References).Returns(assemblies);

                var cmd = new ReferencesCommand();
                var result = cmd.Execute(_repl.Object, null);

                var expected = new List<string> { typeof(string).Assembly.FullName, "path1", "path2" };

                ((IEnumerable<string>)result).ToList().ShouldEqual(expected);
            }
        }
    }
}
