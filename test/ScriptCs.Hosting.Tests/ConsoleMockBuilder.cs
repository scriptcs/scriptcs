using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.Tests
{
    public class ConsoleMockBuilder
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

        internal void Add(ConsoleKey key)
        {
            _chars.Add(new ConsoleKeyInfo(' ', key, false, false, false));
        }

        internal Mock<IConsole> Mock { get { return BuildMock(); } }

        private Mock<IConsole> BuildMock()
        {
            var mock = new Mock<IConsole>();

            mock.Setup(c => c.ReadKey()).Returns(() => _chars.ElementAt(_position++));
            mock.Setup(c => c.Width).Returns(80);

            return mock;
        }
    }
}