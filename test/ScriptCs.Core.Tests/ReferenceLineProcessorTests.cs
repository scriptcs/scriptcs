using System;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using Should;
using Xunit.Extensions;
using System.Linq;
using Should.Core.Assertions;
using ScriptCs.Contracts.Exceptions;

namespace ScriptCs.Tests
{
    public class ReferenceLineProcessorTests
    {
        public class TheProcessLineMethod : IDisposable
        {
            private const string EnvVarKey = "scriptcs";

            private const string EnvVarValue = "Awesomeness!";

            public TheProcessLineMethod()
            {
                Environment.SetEnvironmentVariable(EnvVarKey, EnvVarValue);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnTrueOnReferenceLine(IFileParser parser, ReferenceLineProcessor processor)
            {
                // Arrange
                const string Line = @"#r ""MyDll.dll""";

                // Act
                var result = processor.ProcessLine(parser, new FileParserContext(), Line, true);

                // Assert
                result.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnFalseOtherwise(IFileParser parser, ReferenceLineProcessor processor)
            {
                // Arrange
                const string Line = @"var x = new Test();";

                // Act
                var result = processor.ProcessLine(parser, new FileParserContext(), Line, true);

                // Assert
                result.ShouldBeFalse();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldThrowExceptionIfAfterCode(
                [Frozen] Mock<IFileSystem> fileSystem,
                ReferenceLineProcessor processor,
                IFileParser parser)
            {
                // Arrange
                var context = new FileParserContext();

                const string RelativePath = "..\\script.csx";
                const string Line = @"#r " + RelativePath;
                const string FullPath = "C:\\script.csx";

                fileSystem.Setup(x => x.GetFullPath(RelativePath)).Returns(FullPath);
                
                // Act / Assert
                Assert.Throws(typeof(InvalidDirectiveUseException), () => processor.ProcessLine(parser, context, Line, false));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddReferenceToContext(
                [Frozen] Mock<IFileSystem> fileSystem,
                ReferenceLineProcessor processor,
                IFileParser parser)
            {
                // Arrange
                var context = new FileParserContext();

                const string RelativePath = "..\\Assembly.dll";
                const string Line = @"#r " + RelativePath;
                const string FullPath = "C:\\Assembly.dll";

                fileSystem.Setup(x => x.GetFullPath(RelativePath)).Returns(FullPath);

                // Act
                processor.ProcessLine(parser, context, Line, true);

                // Assert
                context.References.Count.ShouldEqual(1);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddReferenceByNameFromGACIfLocalFileDoesntExist(
                [Frozen] Mock<IFileSystem> fileSystem,
                ReferenceLineProcessor processor,
                IFileParser parser)
            {
                // Arrange
                var context = new FileParserContext();

                var name = "script.csx";
                var line = @"#r " + name;
                var fullPath = "C:\\script.csx";

                fileSystem.Setup(x => x.GetFullPath(name)).Returns(fullPath);
                fileSystem.Setup(x => x.FileExists(fullPath)).Returns(false);

                // Act
                processor.ProcessLine(parser, context, line, true);

                // Assert
                context.References.Count(x => x == name).ShouldEqual(1);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldExpandEnvironmentVariables(
                [Frozen] Mock<IFileSystem> fileSystem,
                ReferenceLineProcessor processor,
                IFileParser parser)
            {
                // Arrange
                var context = new FileParserContext();
                var line = string.Format("#r %{0}%", EnvVarKey);

                // Act
                processor.ProcessLine(parser, context, line, true);

                // Assert
                fileSystem.Verify(x => x.GetFullPath(EnvVarValue));
            }

            public void Dispose()
            {
                Environment.SetEnvironmentVariable(EnvVarKey, null);
            }
        }
    }
}