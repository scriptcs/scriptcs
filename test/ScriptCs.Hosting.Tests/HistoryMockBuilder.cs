using System.Collections.Generic;
using System.Linq;
using Moq;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.Tests
{
    public class HistoryMockBuilder
    {
        private readonly List<string> _history = new List<string>();
        private int _position = 0;
        private bool _freshLine = false;

        internal Mock<IReplHistory> Mock { get { return BuildMock(); } }

        private Mock<IReplHistory> BuildMock()
        {
            var mock = new Mock<IReplHistory>();

            mock.Setup(rh => rh.AddLine(It.IsAny<string>())).Callback((string list) =>
            {
                _history.Add(list);
                _position = _history.Count - 1;
                _freshLine = true;
            });

            mock.Setup(rh => rh.GetNextLine()).Returns(() =>
            {
                _freshLine = false;
                return _history.ElementAt(++_position);
            });

            mock.Setup(rh => rh.GetPreviousLine()).Returns(() =>
            {
                if (!_freshLine)
                    _position--;
                _freshLine = false;
                return _history.ElementAt(_position);
            });

            return mock;
        }
    }
}