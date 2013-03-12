using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace ScriptCs.Tests
{
    public class FileProcessorTests
    {
        private readonly List<string> _file1 = new List<string>
            {
                "using System;",
                @"Console.WriteLine(""Hello Script 1"");",
                @"Console.WriteLine(""Loading Script 2"");",
                @"#load ""script2.csx""",
                @"Console.WriteLine(""Loaded Script 2"");",
                @"Console.WriteLine(""Loading Script 4"");",
                @"#load ""script4.csx"";",
                @"Console.WriteLine(""Loaded Script 4"");",
                @"Console.WriteLine(""Goodbye Script 1"");"
            };

        private readonly List<string> _file2 = new List<string>
            {
                "using System;",
                @"Console.WriteLine(""Hello Script 2"");",
                @"Console.WriteLine(""Loading Script 3"");",
                @"#load ""script3.csx""",
                @"Console.WriteLine(""Loaded Script 3"");",
                @"Console.WriteLine(""Goodbye Script 2"");"
            };

        private readonly List<string> _file3 = new List<string>
            {
                "using System;",
                "using System.Collections.Generic;",
                @"Console.WriteLine(""Hello Script 3"");",
                @"Console.WriteLine(""Goodbye Script 3"");"
            };

        private readonly List<string> _file4 = new List<string>
            {
                "using System;",
                "using System.Core;",
                @"Console.WriteLine(""Hello Script 4"");",
                @"Console.WriteLine(""Goodbye Script 4"");"
            };

        private readonly List<string> _file5 = new List<string>
            {
                "using System.Collections.Generic;",
                @"#r ""PresentationCore""",
                @"Console.WriteLine(""Hello Script 5"");"
            };

        private readonly Mock<IFileSystem> _fileSystem;

        public FileProcessorTests()
        {
            _fileSystem = new Mock<IFileSystem>();
            _fileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script1.csx")))
                       .Returns(_file1.ToArray());
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script2.csx")))
                       .Returns(_file2.ToArray());
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script3.csx")))
                       .Returns(_file3.ToArray());
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script4.csx")))
                        .Returns(_file4.ToArray());
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script5.csx")))
                        .Returns(_file5.ToArray());
        }

        [Fact]
        public void MultipleUsingStatementsShouldProduceDistinctOutput()
        {
            var processor = new FilePreProcessor(_fileSystem.Object);
            var output = processor.ProcessFile("\\script1.csx");

            var splitOutput = output.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i.StartsWith("\\script"))),Times.Exactly(4));
            Assert.Equal(2, splitOutput.Count(x => x.TrimStart(' ').StartsWith("using ")));
        }

        [Fact]
        public void UsingStateMentsShoulAllBeAtTheTop()
        {
            var processor = new FilePreProcessor(_fileSystem.Object);
            var output = processor.ProcessFile("\\script1.csx");

            var splitOutput = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var lastUsing = splitOutput.ToList().FindLastIndex(x => x.TrimStart(' ').StartsWith("using "));
            var firsNotUsing = splitOutput.ToList().FindIndex(x => !x.TrimStart(' ').StartsWith("using "));

            Assert.True(lastUsing < firsNotUsing);
        }

        [Fact]
        public void ThreeLoadedFilesShoulAllBeCalledOnce()
        {
            var processor = new FilePreProcessor(_fileSystem.Object);
            processor.ProcessFile("\\script1.csx");

            _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "\\script1.csx")), Times.Once());
            _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "\\script2.csx")), Times.Once());
            _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "\\script3.csx")), Times.Once());
            _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "\\script4.csx")), Times.Once());
        }

        [Fact]
        public void LoadBeforeUsingShouldBeAllowed()
        {
            var file = new List<string>
                {
                    @"#load ""script4.csx""",
                    "",
                    "using System;",
                    @"Console.WriteLine(""abc"");"
                };
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\file.csx"))).Returns(file.ToArray());

            var processor = new FilePreProcessor(_fileSystem.Object);
            var output = processor.ProcessFile("\\file.csx");

            var splitOutput = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var lastUsing = splitOutput.ToList().FindLastIndex(x => x.TrimStart(' ').StartsWith("using "));
            var firsNotUsing = splitOutput.ToList().FindIndex(x => !x.TrimStart(' ').StartsWith("using "));

            Assert.Equal(2, splitOutput.Count(x => x.TrimStart(' ').StartsWith("using ")));
            Assert.True(lastUsing < firsNotUsing);            
        }

        [Fact]
        public void UsingInCodeDoesNotCountAsUsingImport()
        {
            var file = new List<string>
                {
                    @"#load ""script4.csx""",
                    "",
                    "using System;",
                    "using System.IO;",
                    "Console.WriteLine();",
                    @"using (var stream = new MemoryStream) {",
                    @"//do stuff",
                    @"}"
                };
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\file.csx"))).Returns(file.ToArray());

            var processor = new FilePreProcessor(_fileSystem.Object);
            var output = processor.ProcessFile("\\file.csx");

            var splitOutput = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var firstNonImportUsing = splitOutput.ToList().FindIndex(x => x.TrimStart(' ').StartsWith("using ") && !x.Contains(";"));
            var firstCodeLine = splitOutput.ToList().FindIndex(x => x.Contains("Console"));

            Assert.True(firstNonImportUsing > firstCodeLine);
        }

        [Fact]
        public void ReferencesShouldAlwaysBeOnTop()
        {
            var processor = new FilePreProcessor(_fileSystem.Object);
            var output = processor.ProcessFile("\\script5.csx");

            var splitOutput = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(line => line.Trim(' ')).ToList();

            var lastReference = splitOutput.FindLastIndex(line => line.StartsWith("#r "));
            var firstNotReference = splitOutput.FindIndex(line => !line.StartsWith("#r "));

            Assert.True(lastReference < firstNotReference);
        }
    }
}
