using Ploeh.AutoFixture;
using ScriptCs.Contracts;

namespace ScriptCs.Tests
{
    public class WithoutReplBufferLineAttribute : ScriptCsAutoDataAttribute
    {
        public WithoutReplBufferLineAttribute(params object[] values)
            : base(new ReplBufferCustomization(), values)
        {
        }

        private class ReplBufferCustomization : ICustomization
        {
            public void Customize(IFixture fixture)
            {
                fixture.Customize<IReplBuffer>(b => b.Without(o => o.Line));
            }
        }
    }
}
