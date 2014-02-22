using Ploeh.AutoFixture;
using ScriptCs.Contracts;

namespace ScriptCs.Tests
{
    public class InputLineAutoDataAttribute : ScriptCsAutoDataAttribute
    {
        public InputLineAutoDataAttribute(params object[] values)
            : base(new InputLineCustomization(), values)
        {
        }
    }

    internal class InputLineCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<IReplBuffer>(b => b.Without(o => o.Line));
        }
    }
}
