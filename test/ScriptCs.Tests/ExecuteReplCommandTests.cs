using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.Logging;
using Moq;
using ScriptCs.Command;
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
                var mockFileSystem = new Mock<IFileSystem>();
                mockFileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");

                var builder = new StringBuilder();

                var reader = new StringReader(Environment.NewLine);
                var writer = new StringWriter(builder);

                var keys = new Queue<ConsoleKeyInfo>();
                var enterConsoleKey = new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
                keys.Enqueue(enterConsoleKey);
                var console = new FakeConsole(writer, reader, keys);

                var root = new ScriptServiceRoot(
                    mockFileSystem.Object,
                    Mock.Of<IPackageAssemblyResolver>(),
                    Mock.Of<IScriptExecutor>(),
                    Mock.Of<IScriptEngine>(),
                    Mock.Of<IFilePreProcessor>(),
                    Mock.Of<IScriptPackResolver>(),
                    Mock.Of<IPackageInstaller>(),
                    Mock.Of<ILog>(),
                    console);

                var commandFactory = new CommandFactory(root);

                var target = commandFactory.CreateCommand(new ScriptCsArgs { Repl = true });

                target.Execute();

                Assert.True(builder.ToString().EndsWith(Repl.InputCaret));
                Assert.Equal(0, console.ReadLineCounter);
                Assert.Equal(1, console.ReadKeyCounter);
            }
        }

        public class FakeConsole : IConsole
        {
            private readonly TextWriter writer;
            private readonly TextReader reader;
            private readonly Queue<ConsoleKeyInfo> _consoleKeysToBeRead;

            public FakeConsole(TextWriter textWriter, TextReader textReader, Queue<ConsoleKeyInfo> consoleKeysToBeRead)
            {
                writer = textWriter;
                reader = textReader;
                _consoleKeysToBeRead = consoleKeysToBeRead;
                ReadLineCounter = 0;
                ReadKeyCounter = 0;
            }

            public ConsoleColor ForegroundColor { get; set; }

            public int ReadLineCounter { get; set; }

            public int ReadKeyCounter { get; set; }

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

            public ConsoleKeyInfo ReadKey(bool intercept)
            {
                ReadKeyCounter++;
                var key = _consoleKeysToBeRead.Dequeue();
                if (!intercept)
                    writer.Write(key.KeyChar);
                return key;
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