using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptCsAutoDataAttribute : CompositeDataAttribute
    {
        public ScriptCsAutoDataAttribute(params object[] values)
            : base(
            new InlineDataAttribute(values),
            new InternalScriptCsAutoDataAttribute()
            )
        {
        }
    }

    public class InternalScriptCsAutoDataAttribute : AutoDataAttribute
    {
        public InternalScriptCsAutoDataAttribute() : base(() => new Fixture().Customize(new ScriptCsMoqCustomization()))
        {
        }
    }
}