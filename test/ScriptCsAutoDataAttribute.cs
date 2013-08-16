using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;

namespace ScriptCs.Tests
{
    public class ScriptCsAutoDataAttribute : AutoDataAttribute
    {
        public ScriptCsAutoDataAttribute()
            : base(new Fixture().Customize(new AutoMoqCustomization()))
        {
        }
    }
}