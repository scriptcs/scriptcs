using Moq;
using Ploeh.AutoFixture;
using ScriptCs.Contracts;

namespace ScriptCs.Tests
{
    public class WithoutReplBufferLineButWithConsoleWidthAttribute : ScriptCsAutoDataAttribute
    {
        public WithoutReplBufferLineButWithConsoleWidthAttribute(params object[] values)
            : base(new ReplBufferCustomization(), values)
        {
        }

        private class ReplBufferCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customize<IReplBuffer>(b => b.Without(o => o.Line));
                var consoleMock = new Mock<IConsole>();
                consoleMock.Setup(c => c.Width).Returns(80);
                fixture.Inject<Mock<IConsole>>(consoleMock);
            }
        }
    }
}
