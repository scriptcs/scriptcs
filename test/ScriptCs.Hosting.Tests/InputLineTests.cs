using System;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Moq;
using Xunit.Extensions;

namespace ScriptCs.Hosting.Tests
{
    public class InputLineTests
    {
        public class TheReadLineMethod
        {
            private readonly IScriptExecutor _scriptExec;

            public TheReadLineMethod()
            {
                var fileSystemMock = new Mock<IFileSystem>();
                var scriptExecutorMock = new Mock<IScriptExecutor>();
                scriptExecutorMock.Setup(se => se.FileSystem).Returns(fileSystemMock.Object);
                _scriptExec = scriptExecutorMock.Object;
            }

            [Theory, WithMockBuilders]
            public void ShouldReturnLineAtEnter(string str1, string str2, ConsoleMockBuilder consoleMockBuilder, InputLine inputLine)
            {
                consoleMockBuilder.Add(str1);
                consoleMockBuilder.Add(ConsoleKey.Enter);
                consoleMockBuilder.Add(str2);

                var line = inputLine.ReadLine(_scriptExec);

                line.ShouldEqual(str1);
            }

            [Theory, WithMockBuilders]
            public void ShouldAddLineToHistory(string str, ConsoleMockBuilder consoleMockBuilder, Mock<IReplHistory> historyMock, InputLine inputLine)
            {
                consoleMockBuilder.Add(str);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                historyMock.Verify(h => h.AddLine(str), Times.Once());
            }

            [Theory, WithMockBuilders]
            public void ShouldReturnPreviousLineOnUpArrow(string str1, string str2, ConsoleMockBuilder consoleMockBuilder, InputLine inputLine)
            {
                consoleMockBuilder.Add(str1);
                consoleMockBuilder.Add(ConsoleKey.Enter);
                consoleMockBuilder.Add(str2);
                consoleMockBuilder.Add(ConsoleKey.Enter);
                consoleMockBuilder.Add(ConsoleKey.UpArrow);
                consoleMockBuilder.Add(ConsoleKey.UpArrow);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);
                inputLine.ReadLine(_scriptExec);
                var line = inputLine.ReadLine(_scriptExec);

                line.ShouldEqual(str1);
            }

            [Theory, WithMockBuilders]
            public void ShouldReturnNextLineOnDownArrow(string str1, string str2, string str3, ConsoleMockBuilder consoleMockBuilder, InputLine inputLine)
            {
                consoleMockBuilder.Add(str1);
                consoleMockBuilder.Add(ConsoleKey.Enter);
                consoleMockBuilder.Add(str2);
                consoleMockBuilder.Add(ConsoleKey.Enter);
                consoleMockBuilder.Add(str3);
                consoleMockBuilder.Add(ConsoleKey.Enter);
                consoleMockBuilder.Add(ConsoleKey.UpArrow);
                consoleMockBuilder.Add(ConsoleKey.UpArrow);
                consoleMockBuilder.Add(ConsoleKey.UpArrow);
                consoleMockBuilder.Add(ConsoleKey.DownArrow);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);
                inputLine.ReadLine(_scriptExec);
                inputLine.ReadLine(_scriptExec);
                var line = inputLine.ReadLine(_scriptExec);

                line.ShouldEqual(str2);
            }

            [Theory, WithMockBuilders]
            public void ShouldMoveCursorLeftOnLeftArrow(string str, ConsoleMockBuilder consoleMockBuilder, Mock<IReplBuffer> bufferMock, InputLine inputLine)
            {
                consoleMockBuilder.Add(str);
                consoleMockBuilder.Add(ConsoleKey.LeftArrow);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                bufferMock.Verify(b => b.MoveLeft(), Times.Once());
            }

            [Theory, WithMockBuilders]
            public void ShouldMoveCursorRightOnRightArrow(string str, ConsoleMockBuilder consoleMockBuilder, Mock<IReplBuffer> bufferMock, InputLine inputLine)
            {
                consoleMockBuilder.Add(str);
                consoleMockBuilder.Add(ConsoleKey.RightArrow);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                bufferMock.Verify(b => b.MoveRight(), Times.Once());
            }

            [Theory, WithMockBuilders("foobar")]
            public void ShouldInsertCharacterAtCursor(string str, ConsoleMockBuilder consoleMockBuilder, InputLine inputLine)
            {
                consoleMockBuilder.Add(str);
                consoleMockBuilder.Add(ConsoleKey.LeftArrow);
                consoleMockBuilder.Add('x');
                consoleMockBuilder.Add(ConsoleKey.Enter);

                var line = inputLine.ReadLine(_scriptExec);

                line.ShouldEqual("foobaxr");
            }

            [Theory, WithMockBuilders]
            public void ShouldBackCursorOnBackspace(string str, ConsoleMockBuilder consoleMockBuilder, Mock<IReplBuffer> bufferMock, InputLine inputLine)
            {
                consoleMockBuilder.Add(str);
                consoleMockBuilder.Add(ConsoleKey.Backspace);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                bufferMock.Verify(b => b.Back(), Times.Once());
            }

            [Theory, WithMockBuilders]
            public void ShouldDeleteOneCharacterOnDelete(string str, ConsoleMockBuilder consoleMockBuilder, Mock<IReplBuffer> bufferMock, InputLine inputLine)
            {
                consoleMockBuilder.Add(str);
                consoleMockBuilder.Add(ConsoleKey.LeftArrow);
                consoleMockBuilder.Add(ConsoleKey.LeftArrow);
                consoleMockBuilder.Add(ConsoleKey.Delete);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                bufferMock.Verify(b => b.Delete(), Times.Once());
            }

            [Theory, WithMockBuilders]
            public void ShouldNotDeleteCharacterWhenAtEndOfTheLine(string str, ConsoleMockBuilder consoleMockBuilder, Mock<IReplBuffer> bufferMock, InputLine inputLine)
            {
                consoleMockBuilder.Add(str);
                consoleMockBuilder.Add(ConsoleKey.Delete);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                bufferMock.Verify(b => b.Delete(), Times.Never());
            }

            [Theory, WithMockBuilders]
            public void ShouldNotDeleteCharacterWhenLineIsEmpty(ConsoleMockBuilder consoleMockBuilder, Mock<IReplBuffer> bufferMock, InputLine inputLine)
            {
                consoleMockBuilder.Add(ConsoleKey.Delete);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                bufferMock.Verify(b => b.Delete(), Times.Never());
            }

            [Theory, WithMockBuilders]
            public void ShouldGetWordCompletionOnTabWhenLoadingAssembly(ConsoleMockBuilder consoleMockBuilder, [Frozen] Mock<ILineAnalyzer> lineAnalyserMock, [Frozen] Mock<ICompletionHandler> completionMock, InputLine inputLine)
            {
                lineAnalyserMock.Setup(la => la.CurrentState).Returns(LineState.AssemblyName);

                consoleMockBuilder.Add(ConsoleKey.Tab);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                completionMock.Verify(c => c.UpdateBufferWithCompletion(It.IsAny<Func<string, string[]>>()), Times.Once());
            }

            [Theory, WithMockBuilders]
            public void ShouldGetWordCompletionOnTabWhenEnteringFilePath(ConsoleMockBuilder consoleMockBuilder, [Frozen] Mock<ILineAnalyzer> lineAnalyserMock, [Frozen] Mock<ICompletionHandler> completionMock, InputLine inputLine)
            {
                lineAnalyserMock.Setup(la => la.CurrentState).Returns(LineState.FilePath);

                consoleMockBuilder.Add(ConsoleKey.Tab);
                consoleMockBuilder.Add(ConsoleKey.Enter);

                inputLine.ReadLine(_scriptExec);

                completionMock.Verify(c => c.UpdateBufferWithCompletion(It.IsAny<Func<string, string[]>>()), Times.Once());
            }
        }
    }
}
