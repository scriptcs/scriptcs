using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            private readonly Mock<IConsole> _console;
            private readonly Mock<IRepl> _repl;
            private readonly Mock<ScriptPackSession> _scriptPackSession;

            public ExecuteMethod()
            {
                _console = new Mock<IConsole>();
                _repl = new Mock<IRepl>();
                _scriptPackSession = new Mock<ScriptPackSession>(Enumerable.Empty<IScriptPack>(), new string[0]);

                _scriptPackSession.Setup(x => x.Contexts).Returns(new List<IScriptPackContext> { new DummyScriptPack() });
                _repl.Setup(x => x.ScriptPackSession).Returns(_scriptPackSession.Object);
                _repl.Setup(x => x.Namespaces).Returns(new Collection<string>());
            }

            [Fact]
            public void ShouldExitIfThereAreNoScriptPacks()
            {
                // arrange
                _scriptPackSession.Setup(x => x.Contexts).Returns((IEnumerable<IScriptPackContext>)null);
                var cmd = new ScriptPacksCommand(_console.Object);

                // act
                cmd.Execute(_repl.Object, null);

                // assert
                _console.Verify(x => x.WriteLine("There are no script packs available in this REPL session"));
            }

            [Fact]
            public void ShouldPrintMethodSignatures()
            {
                // arrange
                var cmd = new ScriptPacksCommand(_console.Object);

                // act
                cmd.Execute(_repl.Object, null);

                // assert
                _console.Verify(x => x.WriteLine(typeof(DummyScriptPack).FullName.ToString()));
                _console.Verify(x => x.WriteLine("** Methods **"));
                _console.Verify(x => x.WriteLine(" - string Foo(int bar)"));
                _console.Verify(x => x.WriteLine(" - ScriptCs.Tests.ReplCommands.DummyScriptPack Something()"));
            }

            [Fact]
            public void ShouldPrintPropertyInformation()
            {
                // arrange
                var cmd = new ScriptPacksCommand(_console.Object);

                // act
                cmd.Execute(_repl.Object, null);

                // assert
                _console.Verify(x => x.WriteLine(typeof(DummyScriptPack).FullName.ToString()));
                _console.Verify(x => x.WriteLine("** Properties **"));
                _console.Verify(x => x.WriteLine(" - double FooBar { get; set; }"));
                _console.Verify(x => x.WriteLine(" - ScriptCs.Tests.ReplCommands.DummyScriptPack Xyz { get; }"));
            }

            [Fact]
            public void ShouldRespectNamespaces()
            {
                // arrange
                _repl.Setup(x => x.Namespaces).Returns(new Collection<string> { "ScriptCs.Tests.ReplCommands" });
                var cmd = new ScriptPacksCommand(_console.Object);

                // act
                cmd.Execute(_repl.Object, null);

                // assert
                _console.Verify(x => x.WriteLine(" - DummyScriptPack Something()"));
                _console.Verify(x => x.WriteLine(" - DummyScriptPack Xyz { get; }"));
            }
        }
    }

    internal class DummyScriptPack : IScriptPackContext
    {
        public string Foo(int bar)
        {
            return bar.ToString();
        }

        public DummyScriptPack Something()
        {
            return null;
        }

        public double FooBar { get; set; }

        public DummyScriptPack Xyz
        {
            get { return null; }
        }
    }

    internal static class DummyScriptPackExtensions
    {
        public static void FooExtension(this DummyScriptPack dummyScriptPack)
        {
        }
    }
}
