using System;
using System.IO;
using System.Text;
using Common.Logging;
using Moq;

using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Package;
using Xunit;

namespace ScriptCs.Tests
{
    public class ExecuteReplCommandTests
    {
        public class TheExecuteMethod
        {
            [Fact]
            public void ShouldPromptForInput()
            {
                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var mockFileSystem = new Mock<IFileSystem>();
                mockFileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");
                fixture.Register(() => mockFileSystem.Object);

                var builder = new StringBuilder();

                var reader = new StringReader(Environment.NewLine);
                var writer = new StringWriter(builder);

                var console = new FakeConsole(writer, reader);
                fixture.Register<IConsole>(() => console);

                var root = fixture.Create<ScriptServiceRoot>();

                var commandFactory = new CommandFactory(root);

                var target = commandFactory.CreateCommand(new ScriptCsArgs { Repl = true }, new string[0]);

                target.Execute();

                Assert.True(builder.ToString().EndsWith("> "));
                Assert.Equal(1, console.ReadLineCounter);
            }
        }

        public class FakeConsole : IConsole
        {
            private readonly TextWriter writer;
            private readonly TextReader reader;

            public FakeConsole(TextWriter textWriter, TextReader textReader)
            {
                writer = textWriter;
                reader = textReader;
                ReadLineCounter = 0;
            }

            public ConsoleColor ForegroundColor { get; set; }

            public int ReadLineCounter { get; set; }

            public void Write(string value)
            {
                writer.Write(value);
            }

            public void WriteLine(string value)
            {
                writer.WriteLine(value);
            }

            public string ReadLine()
            {
                ReadLineCounter++;
                return reader.ReadLine();
            }

            public void Exit()
            {
            }

            public void ResetColor()
            {
            }
        }
    }
}
