using Moq;
using Ploeh.AutoFixture;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Tests;

namespace ScriptCs.Tests
{
    public class WithMockBuildersAttribute : ScriptCsAutoDataAttribute
    {
        public WithMockBuildersAttribute(params object[] values)
            : base(new ConsoleMockBuildCustomization(), values)
        {
        }

        private class ConsoleMockBuildCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                var consoleBuilder = new ConsoleMockBuilder();
                fixture.Inject<ConsoleMockBuilder>(consoleBuilder);
                fixture.Inject<Mock<IConsole>>(consoleBuilder.Mock);

                var historyBuilder = new HistoryMockBuilder();
                fixture.Inject<HistoryMockBuilder>(historyBuilder);
                fixture.Inject<Mock<IReplHistory>>(historyBuilder.Mock);

                var bufferBuilder = new ReplBufferMockBuilder();
                fixture.Inject<ReplBufferMockBuilder>(bufferBuilder);
                fixture.Inject<Mock<IReplBuffer>>(bufferBuilder.Mock);
            }
        }
    }
}
