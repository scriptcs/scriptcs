using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;

namespace ScriptCs.Tests
{
    public class ScriptCsAutoDataAttribute : AutoDataAttribute
    {
        public ScriptCsAutoDataAttribute() : this(new Fixture()) { }

        private ScriptCsAutoDataAttribute(IFixture fixture) : base(fixture.Customize(new AutoMoqCustomization())) { }
    }
}