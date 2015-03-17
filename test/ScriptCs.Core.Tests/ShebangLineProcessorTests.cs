using Should;
using Xunit.Extensions;
using ScriptCs.Contracts;

namespace ScriptCs.Tests
{
    public class ShebangLineProcessorTests
    {
        public class TheProcessLineMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnTrueOnShebangLine(IFileParser parser, ShebangLineProcessor processor)
            {
                // Arrange
                const string Line = @"#!/usr/bin/env scriptcs";

                // Act
                var result = processor.ProcessLine(parser, new FileParserContext(), Line, true);

                // Assert
                result.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnFalseOtherwise(IFileParser parser, ShebangLineProcessor processor)
            {
                // Arrange
                const string Line = @"var x = new Test();";

                // Act
                var result = processor.ProcessLine(parser, new FileParserContext(), Line, true);

                // Assert
                result.ShouldBeFalse();
            }
        }
    }
}