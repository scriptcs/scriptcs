using Should;
using Xunit;
using System;

namespace ScriptCs.Tests
{
    public class StringExtensionTests
    {
        public class TheSplitQuotedMethod
        {
            [Fact]
            public void ShouldKeepQuotedStringTogether()
            {
                string line = ":cd \"\\\\Foo Bar\"";
                var result = line.SplitQuoted();

                result.Length.ShouldEqual(2);
                result[0].ShouldEqual(":cd");
                result[1].ShouldEqual("\"\\\\Foo Bar\"");
            }

            [Fact]
            public void MissingQuoteThrowException()
            {
                string line = ":alias \"clear\" \"wipe";

                Assert.Throws(typeof(ArgumentException), () => line.SplitQuoted());
            }
        }
    }
}
