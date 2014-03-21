﻿using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class ScriptCsAutoDataAttribute : CompositeDataAttribute
    {
        public ScriptCsAutoDataAttribute(params object[] values)
            : base(
            new InlineDataAttribute(values),
            new AutoDataAttribute(
                new Fixture().Customize(new AutoMoqCustomization()))
            )
        {
        }

        internal ScriptCsAutoDataAttribute(ICustomization cust, params object[] values)
            : base(
            new InlineDataAttribute(values),
            new AutoDataAttribute(
                new Fixture().Customize(cust).Customize(new AutoMoqCustomization()))
            )
        {
        }
    }
}