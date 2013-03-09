using Moq;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DebugFilePreProcessorTests
    {
        private static DebugFilePreProcessor CreateFilePreProcessor(Mock<IFileSystem> fileSystem = null)
        {
            fileSystem = fileSystem ?? new Mock<IFileSystem>();

            return new DebugFilePreProcessor(fileSystem.Object);
        }

        public class TheParseFileMethod
        {
            private List<string> fileWithoutSystemDiagnostics = new List<string>
            {
                "using System;",
                "using System.Collections.Generic;",
                @"Console.WriteLine(""Hello Script 3"");",
                @"Console.WriteLine(""Goodbye Script 3"");"
            };

            private List<string> fileWithSystemDiagnostics = new List<string>
            {
                "using System;",
                "using System.Diagnostics;",
                "using System.Collections.Generic;",
                @"Console.WriteLine(""Hello Script 3"");",
                @"Debug.WriteLine(""Hello Script 3"");",
                @"Console.WriteLine(""Goodbye Script 3"");"
            };

            private Mock<IFileSystem> _fileSystem;

            public TheParseFileMethod()
            {
                _fileSystem = new Mock<IFileSystem>();
                _fileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
            }

            [Fact]
            public void ShouldAddSystemDiagnosticsUsingIfItDoesNotExistInOriginalCode()
            {
                // arrange
                const string FilePath = "C:\filePath.csx";
                _fileSystem.Setup(fs => fs.ReadFileLines(FilePath)).Returns(this.fileWithoutSystemDiagnostics.ToArray());

                var preProcessor = CreateFilePreProcessor(_fileSystem);
                
                // act
                var processedCode = preProcessor.ProcessFile(FilePath);

                // assert
                processedCode.ShouldContain("using System.Diagnostics;");
            }

            [Fact]
            public void ShouldNotAddSystemDiagnosticsUsingIfItExistsInOriginalCode()
            {
                // arrange
                const string FilePath = "C:\filePath.csx";
                _fileSystem.Setup(fs => fs.ReadFileLines(FilePath)).Returns(this.fileWithSystemDiagnostics.ToArray());

                var preProcessor = CreateFilePreProcessor(_fileSystem);

                // act
                var processedCode = preProcessor.ProcessFile(FilePath);

                // assert
                var lines = processedCode.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                lines.Count(l => l.Equals("using System.Diagnostics;")).ShouldEqual(1);
            }
        }
    }
}