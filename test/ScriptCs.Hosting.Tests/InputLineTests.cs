using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using ScriptCs.Contracts;
using Should;
using Xunit;
using Moq;

namespace ScriptCs.Hosting.Tests
{
    public class InputLineTests
    {
        public class TheReadLineMethod
        {
            private readonly IInputLine _inputLine;
            private readonly Mock<ILineAnalyzer> _lineAnalyzerMock;
            private readonly Mock<IReplHistory> _replHistoryMock;
            private readonly HistoryMockBuilder _hmb = new HistoryMockBuilder();
            private readonly Mock<IReplBuffer> _replBufferMock;
            private readonly StringBuilder _buffer; 
            private readonly Mock<IFileSystem> _fileSystemMock;
            private readonly Mock<IScriptExecutor> _scriptExecutorMock;
            private readonly ConsoleMockBuilder _cmb = new ConsoleMockBuilder();
            private readonly Mock<IConsole> _consoleMock; 

            public TheReadLineMethod()
            {
                _lineAnalyzerMock = new Mock<ILineAnalyzer>();
                _replHistoryMock = _hmb.Mock;
                _replBufferMock = new Mock<IReplBuffer>();
                _buffer = new StringBuilder();
                _fileSystemMock = new Mock<IFileSystem>();
                _scriptExecutorMock = new Mock<IScriptExecutor>();
                _scriptExecutorMock.Setup(se => se.FileSystem).Returns(_fileSystemMock.Object);
                _consoleMock = _cmb.Mock;

                _inputLine = new InputLine(_lineAnalyzerMock.Object, _replHistoryMock.Object, _replBufferMock.Object, _consoleMock.Object);
            }

            [Fact]
            public void ShouldReturnLineAtEnter()
            {
                SetBufferToEcho();

                const string testString = "foo";

                _cmb.Add(testString);
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));
                _cmb.Add("bar");

                var line = _inputLine.ReadLine(_scriptExecutorMock.Object);

                line.ShouldEqual(testString);
            }

            [Fact]
            public void ShouldAddLineToHistory()
            {
                SetBufferToEcho();

                const string testString = "foo";

                _cmb.Add(testString);
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));

                _inputLine.ReadLine(_scriptExecutorMock.Object);

                _replHistoryMock.Verify(rh => rh.AddLine(testString), Times.Once());
            }

            [Fact]
            public void ShouldReturnPreviousLineOnUpArrow()
            {
                SetBufferToEcho();

                const string testString1 = "foo";
                const string testString2 = "bar";

                _cmb.Add(testString1);
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));
                _cmb.Add(testString2);
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));
                _cmb.Add(BuildKeyInfo(ConsoleKey.UpArrow));
                _cmb.Add(BuildKeyInfo(ConsoleKey.UpArrow));
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));

                _inputLine.ReadLine(_scriptExecutorMock.Object);
                _inputLine.ReadLine(_scriptExecutorMock.Object);
                var line = _inputLine.ReadLine(_scriptExecutorMock.Object);

                line.ShouldEqual(testString1);
            }

            [Fact]
            public void ShouldReturnNextLineOnDownArrow()
            {
                SetBufferToEcho();

                const string testString1 = "foo";
                const string testString2 = "bar";
                const string testString3 = "ninja";

                _cmb.Add(testString1);
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));
                _cmb.Add(testString2);
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));
                _cmb.Add(testString3);
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));
                _cmb.Add(BuildKeyInfo(ConsoleKey.UpArrow));
                _cmb.Add(BuildKeyInfo(ConsoleKey.UpArrow));
                _cmb.Add(BuildKeyInfo(ConsoleKey.UpArrow));
                _cmb.Add(BuildKeyInfo(ConsoleKey.DownArrow));
                _cmb.Add(BuildKeyInfo(ConsoleKey.Enter));

                _inputLine.ReadLine(_scriptExecutorMock.Object);
                _inputLine.ReadLine(_scriptExecutorMock.Object);
                _inputLine.ReadLine(_scriptExecutorMock.Object);
                var line = _inputLine.ReadLine(_scriptExecutorMock.Object);

                line.ShouldEqual(testString2);
            }

            private void SetBufferToEcho()
            {
                _replBufferMock.Setup(rb => rb.StartLine()).Callback(() => _buffer.Clear());
                _replBufferMock.SetupSet(rb => rb.Line = It.IsAny<string>()).Callback((string str) =>
                {
                    _buffer.Clear();
                    _buffer.Append(str);
                });
                _replBufferMock.Setup(rb => rb.Insert(It.IsAny<char>())).Callback((char c) => _buffer.Append(c));
                _replBufferMock.Setup(rb => rb.Line).Returns(() => _buffer.ToString());
            }

            private ConsoleKeyInfo BuildKeyInfo(ConsoleKey key)
            {
                return new ConsoleKeyInfo(' ', key, false, false, false);
            }
        }
       
    }

    internal class HistoryMockBuilder
    {
        private readonly List<string> _history = new List<string>();
        private int _position = 0;
        private bool freshLine = false;

        internal Mock<IReplHistory> Mock { get { return BuildMock(); } }

        private Mock<IReplHistory> BuildMock()
        {
            var mock = new Mock<IReplHistory>();

            mock.Setup(rh => rh.AddLine(It.IsAny<string>())).Callback((string list) =>
            {
                _history.Add(list);
                _position = _history.Count - 1;
                freshLine = true;
            });
            mock.Setup(rh => rh.GetNextLine()).Returns(() =>
            {
                freshLine = false;
                return _history.ElementAt(++_position);
            });
            mock.Setup(rh => rh.GetPreviousLine()).Returns(() =>
            {
                if (!freshLine)
                    _position--;
                freshLine = false;
                return _history.ElementAt(_position);
            });

            return mock;
        }
    }

    internal class ConsoleMockBuilder
    {
        private readonly IList<ConsoleKeyInfo> _chars = new List<ConsoleKeyInfo>(); 
        private int _position = 0;

        internal void Add(ConsoleKeyInfo keyInfo)
        {
            _chars.Add(keyInfo);
        }

        internal void Add(char ch)
        {
            _chars.Add(new ConsoleKeyInfo(ch, ConsoleKey.Zoom /* Dummy */, false, false, false));
        }

        internal void Add(string str)
        {
            foreach (var c in str)
            {
                Add(c);
            }
        }

        internal Mock<IConsole> Mock { get { return BuildMock(); } }

        private Mock<IConsole> BuildMock()
        {
            var mock = new Mock<IConsole>();

            mock.Setup(c => c.ReadKey()).Returns(() => _chars.ElementAt(_position++));

            return mock;
        }
    }
}
