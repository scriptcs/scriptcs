using ScriptCs.Contracts;

using Should;

using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class UsingLineProcessorTests
    {
        public class TheProcessLineMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnTrueOnUsingLine(IFileParser parser, UsingLineProcessor processor)
            {
                // Arrange
                const string UsingLine = @"using ""System.Data"";";

                // Act
                var result = processor.ProcessLine(parser, new FileParserContext(), UsingLine, true);

                // Assert
                result.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnFalseOtherwise(IFileParser parser, UsingLineProcessor processor)
            {
                // Arrange
                const string UsingLine = @"using (var x = new Disposable())";

                // Act
                var result = processor.ProcessLine(parser, new FileParserContext(), UsingLine, true);

                // Assert
                result.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldIgnoreAliases(IFileParser parser, UsingLineProcessor processor)
            {
                // Arrange
                const string UsingLine = @"using Path = ""System.IO.Path"";";

                // Act
                var result = processor.ProcessLine(parser, new FileParserContext(), UsingLine, true);

                // Assert
                result.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddNamespaceToContext(IFileParser parser, UsingLineProcessor processor)
            {
                // Arrange
                const string UsingLine = @"using ""System.Data"";";
                var context = new FileParserContext();

                // Act
                processor.ProcessLine(parser, context, UsingLine, true);

                // Assert
                context.Namespaces.Count.ShouldEqual(1);
            }
        }
    }
}