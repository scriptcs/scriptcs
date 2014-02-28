using System.Text;
using Moq;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.Tests
{
    public class ReplBufferMockBuilder
    {
        private readonly StringBuilder _buffer = new StringBuilder();
        private int _position = 0;

        internal Mock<IReplBuffer> Mock { get { return BuildMock(); } }

        private Mock<IReplBuffer> BuildMock()
        {
            var mock = new Mock<IReplBuffer>();

            mock.Setup(rb => rb.StartLine()).Callback(() => { _buffer.Clear(); _position = 0; });

            mock.SetupSet(rb => rb.Line = It.IsAny<string>()).Callback((string str) =>
            {
                _buffer.Clear();
                _buffer.Append(str);
                _position = str.Length;
            });

            mock.Setup(rb => rb.Insert(It.IsAny<char>())).Callback((char c) => { _buffer.Append(c); _position++; });

            mock.Setup(rb => rb.Line).Returns(() => _buffer.ToString());

            mock.Setup(rb => rb.Position).Returns(() => _position);

            return mock;
        }
    }
}
