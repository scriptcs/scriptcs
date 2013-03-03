using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Xunit;

namespace Scriptcs.Tests
{
    public class FileProcessorTests
    {
        private string file1 = @"using System;|Console.WriteLine(""Hello Script 1"");|Console.WriteLine(""Loading Script 2"");|#load ""script2.csx"";|Console.WriteLine(""Loaded Script 2"");Console.WriteLine(""Loading Script 4"");|#load ""script4.csx"";| Console.WriteLine(""Loaded Script 4"");| Console.WriteLine(""Goodbye Script 1"");|";
        private string file2 = @"using System;|Console.WriteLine(""Hello Script 2"");|    Console.WriteLine(""Loading Script 3"");|    #load ""script3.csx"";|  Console.WriteLine(""Goodbye Script 2"");";
        private string file3 = @"using System;|  Console.WriteLine(""Hello Script 3"");|  Console.WriteLine(""Goodbye Script 3"");|";
        private string file4 = @"using System;|using System.Core;|    Console.WriteLine(""Hello Script 4"");|  Console.WriteLine(""Goodbye Script 4"");|";

        private Mock<IFileSystem> _fileSystem;

        public FileProcessorTests()
        {
            _fileSystem = new Mock<IFileSystem>();
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script1.csx")))
                       .Returns(file1.Split('|'));
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script2.csx")))
                       .Returns(file2.Split('|'));
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script3.csx")))
                       .Returns(file3.Split('|'));
            _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script4.csx")))
                        .Returns(file4.Split('|'));
        }

        [Fact]
        public void MergeUsingStateMentsFromMultipleFiles()
        {
            var processor = new FilePreProcessor(_fileSystem.Object);
            var output = processor.ProcessFile("\\script1.csx");

            var splitOutput = output.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i.StartsWith("\\script"))),Times.Exactly(4));
            Assert.Equal(2, splitOutput.Count(x => x.TrimStart(' ').StartsWith("using ")));
        }

        [Fact]
        public void UsingStateMentsAreAtTheTop()
        {
            var processor = new FilePreProcessor(_fileSystem.Object);
            var output = processor.ProcessFile("file1");

            var splitOutput = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var lastUsing = splitOutput.ToList().FindLastIndex(x => x.TrimStart(' ').StartsWith("using "));
            var firsNotUsing = splitOutput.ToList().FindIndex(x => !x.TrimStart(' ').StartsWith("using "));

            Assert.True(lastUsing < firsNotUsing);
        }
    }
}
