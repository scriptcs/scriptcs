using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class FileProcessorTests
    {
        public class TheProcessFileMethod
        {
            private List<string> file1 = new List<string>
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

            private List<string> file2 = new List<string>
            {
                "using System;",
                @"Console.WriteLine(""Hello Script 2"");",
                @"Console.WriteLine(""Loading Script 3"");",
                @"#load ""script3.csx""",
                @"Console.WriteLine(""Loaded Script 3"");",
                @"Console.WriteLine(""Goodbye Script 2"");"
            };

            private List<string> file3 = new List<string>
            {
                "using System;",
                "using System.Collections.Generic;",
                @"Console.WriteLine(""Hello Script 3"");",
                @"Console.WriteLine(""Goodbye Script 3"");"
            };

            private List<string> file4 = new List<string>
            {
                "using System;",
                "using System.Core;",
                @"Console.WriteLine(""Hello Script 4"");",
                @"Console.WriteLine(""Goodbye Script 4"");"
            };

            private readonly Mock<IFileSystem> _fileSystem;

            public TheProcessFileMethod()
            {
                _fileSystem = new Mock<IFileSystem>();
                _fileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script1.csx")))
                            .Returns(file1.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script2.csx")))
                            .Returns(file2.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script3.csx")))
                            .Returns(file3.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "\\script4.csx")))
                            .Returns(file4.ToArray());
            }

            [Fact]
            public void MultipleUsingStatementsShouldProduceDistinctOutput()
            {
                var processor = new FilePreProcessor(_fileSystem.Object);
                var output = processor.ProcessFile("\\script1.csx");

                var splitOutput = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i.StartsWith("\\script"))), Times.Exactly(4));
                Assert.Equal(3, splitOutput.Count(x => x.TrimStart(' ').StartsWith("using ")));
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
            public void ShouldAddLineDirectiveRightAfterLastLoadIsIncludedInEachFile()
            {
                // f1 has usings and then loads
                var f1 = new List<string>
                        {
                            "using System;",
                            "using System.Diagnostics;",
                            @"#load ""C:\f2.csx"";",
                            @"#load ""C:\f3.csx"";",
                            @"Console.WriteLine(""First line of f1"");",
                        };

                // f2 has no usings and multiple loads
                var f2 = new List<string>
                        {
                            @"#load ""C:\f4.csx"";",
                            @"#load ""C:\f5.csx"";",
                            @"Console.WriteLine(""First line of f2"");",
                        };

                // f3 has usings and no loads
                var f3 = new List<string>
                        {
                            @"using System;",
                            @"using System.Diagnostics;",
                            @"Console.WriteLine(""First line of f3"");",
                        };

                // f4 has no usings and no loads
                var f4 = new List<string>
                        {
                            @"Console.WriteLine(""First line of f4"");",
                        };

                // f5 is no special case, just used to be loaded
                var f5 = new List<string>
                        {
                            @"using System;",
                            @"Console.WriteLine(""First line of f5"");",
                        };

                _fileSystem.SetupGet(fs => fs.NewLine).Returns(Environment.NewLine);
                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f1.csx"))
                            .Returns(f1.ToArray());
                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f2.csx"))
                            .Returns(f2.ToArray()).Verifiable();
                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f3.csx"))
                            .Returns(f3.ToArray());
                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f4.csx"))
                            .Returns(f4.ToArray());
                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f5.csx"))
                            .Returns(f5.ToArray());
                _fileSystem.Setup(fs => fs.IsPathRooted(It.IsAny<string>())).Returns(true);

                var preProcessor = new FilePreProcessor(_fileSystem.Object);

                var file = preProcessor.ProcessFile(@"C:\f1.csx");
                
                var fileLines = file.Split(new[]{ Environment.NewLine }, StringSplitOptions.None);

                // using statements go first, after that f4 -> f5 -> f2 -> f3 -> f1
                var line = 0;
                fileLines[line++].ShouldEqual("using System;");
                fileLines[line++].ShouldEqual("using System.Diagnostics;");

                fileLines[line++].ShouldEqual(@"#line 1 ""C:\f4.csx""");
                fileLines[line++].ShouldEqual(f4[0]);

                fileLines[line++].ShouldEqual(@"#line 2 ""C:\f5.csx""");
                fileLines[line++].ShouldEqual(f5[1]);

                fileLines[line++].ShouldEqual(@"#line 3 ""C:\f2.csx""");
                fileLines[line++].ShouldEqual(f2[2]);

                fileLines[line++].ShouldEqual(@"#line 3 ""C:\f3.csx""");
                fileLines[line++].ShouldEqual(f3[2]);

                fileLines[line++].ShouldEqual(@"#line 5 ""C:\f1.csx""");
                fileLines[line].ShouldEqual(f1[4]);
            }
        }
    }
}
