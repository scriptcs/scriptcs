using System.Text;
using Moq;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.Tests
{
    public class ReplBufferMockBuilder
    {
        private StringBuilder buffer = new StringBuilder();

        internal Mock<IReplBuffer> Mock { get { return BuildMock(); } }

        private Mock<IReplBuffer> BuildMock()
        {
            var mock = new Mock<IReplBuffer>();

            mock.Setup(rb => rb.StartLine()).Callback(() => buffer.Clear());

            mock.SetupSet(rb => rb.Line = It.IsAny<string>()).Callback((string str) =>
            {
                buffer.Clear();
                buffer.Append(str);
            });

            mock.Setup(rb => rb.Insert(It.IsAny<char>())).Callback((char c) => buffer.Append(c));

            mock.Setup(rb => rb.Line).Returns(() => buffer.ToString());

            return mock;
        }
    }
}
