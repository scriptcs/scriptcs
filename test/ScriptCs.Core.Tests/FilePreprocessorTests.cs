using System;
using System.Collections.Generic;
using System.Linq;

using Moq;

using Should;

using Xunit;

namespace ScriptCs.Tests
{
    public class FilePreprocessorTests
    {
        private static readonly Mock<IFileSystem> FileSystem;

        static FilePreprocessorTests()
        {
            FileSystem = new Mock<IFileSystem>();
            FileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
        }

        public class UsingStatements
        {
            [Fact]
            public void ShouldBeOnTopIfNoReferencesArePresent()
            {
                var file = new List<string>
                {
                    @"Console.WriteLine(""Testing testing..."");",
                    @"using System;",
                    @"using System.Threading;"
                };

                var usingCount = ProcessFileAndCountUsings(file);
                usingCount.ShouldEqual(2);
            }

            [Fact]
            public void ShouldNotCountAsImportInsideCode()
            {
                var file = new List<string>
                {
                    @"using System;",
                    @"using System.IO;",
                    @"Console.WriteLine(""Testing testing..."");",
                    @"using (var fileStream = File.OpenRead(""file.txt""))",
                    @"{ // Do something here }"
                };

                var usingCount = ProcessFileAndCountUsings(file);
                usingCount.ShouldEqual(2);
            }

            [Fact]
            public void ShouldBeDistinct()
            {
                var file = new List<string>
                {
                    @"using System;",
                    @"using System.IO;",
                    @"using System.IO;",
                    @"using System;",
                    @"Console.WriteLine(""Testing testing..."");",
                    @"using System.IO;",
                };

                var usingCount = ProcessFileAndCountUsings(file);
                usingCount.ShouldEqual(2);
            }

            private static int ProcessFileAndCountUsings(List<string> file)
            {
                FileSystem.Setup(x => x.ReadFileLines(It.IsAny<string>())).Returns(file.ToArray());

                var processor = new FilePreProcessor(FileSystem.Object);
                var output = processor.ProcessFile("\\script.csx");

                var splitOutput = SplitOutput(output);

                return splitOutput.TakeWhile(line => line.StartsWith("using ")).Count();
            }
        }

        public class LoadStatements
        {
            [Fact]
            public void ShouldOnlyBeLoadedOnce()
            {
                var script1 = new List<string>
                {
                    @"#load ""script2.csx""",
                    @"using System;",
                    @"using System.IO;",
                    @"Console.WriteLine(""Testing testing..."");",
                };

                var script2 = new List<string>
                {
                    @"using System.Core;",
                    @"#load ""script1.csx""", // Circular reference
                    @"Console.WriteLine(""Testing testing..."");",
                };

                FileSystem.Setup(x => x.ReadFileLines("\\script1.csx")).Returns(script1.ToArray());
                FileSystem.Setup(x => x.ReadFileLines("\\script2.csx")).Returns(script2.ToArray());

                var processor = new FilePreProcessor(FileSystem.Object);
                var output = processor.ProcessFile("\\script1.csx");

                var splitOutput = SplitOutput(output);

                splitOutput.Count.ShouldEqual(5);
            }
        }

        public class ReferenceStatements
        {
            [Fact]
            public void ShouldAlwaysBeOnTop()
            {
                var script1 = new List<string>
                {
                    @"using System;",
                    @"#r ""WindowsBase""",
                    @"using System.IO;",
                    @"#load ""script2.csx""",
                    @"Console.WriteLine(""Testing testing..."");",
                };

                var script2 = new List<string>
                {
                    @"using System.Core;",
                    @"#r ""PresentationCore""",
                    @"Console.WriteLine(""Testing testing..."");",
                };

                FileSystem.Setup(x => x.ReadFileLines("\\script1.csx")).Returns(script1.ToArray());
                FileSystem.Setup(x => x.ReadFileLines("\\script2.csx")).Returns(script2.ToArray());

                var processor = new FilePreProcessor(FileSystem.Object);
                var output = processor.ProcessFile("\\script1.csx");

                var splitOutput = SplitOutput(output);

                var referenceCount = splitOutput.TakeWhile(line => line.StartsWith("#r ")).Count();
                referenceCount.ShouldEqual(2);
            }
        }

        private static List<string> SplitOutput(string output)
        {
            return output.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                      .Select(line => line.Trim(' '))
                      .ToList();
        }
    }
}