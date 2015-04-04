using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Contracts.Exceptions;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class FileProcessorTests
    {
        public class ProcessFileMethod
        {
            private List<string> _file1 = new List<string>
                {
                    @"#!/usr/bin/env scriptcs",
                    @"#load ""script2.csx""",
                    @"#load ""script4.csx"";",
                    "using System;",
                    @"Console.WriteLine(""Hello Script 1"");",
                    @"Console.WriteLine(""Loading Script 2"");",
                    @"Console.WriteLine(""Loaded Script 2"");",
                    @"Console.WriteLine(""Loading Script 4"");",
                    @"Console.WriteLine(""Loaded Script 4"");",
                    @"Console.WriteLine(""Goodbye Script 1"");"
                };

            private List<string> _file2 = new List<string>
                {
                    "using System;",
                    @"Console.WriteLine(""Hello Script 2"");",
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

            private readonly Mock<IFileSystem> _fileSystem;

            public ProcessFileMethod()
            {
                _fileSystem = new Mock<IFileSystem>();
                _fileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx")))
                           .Returns(_file1.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script2.csx")))
                           .Returns(_file2.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script3.csx")))
                           .Returns(_file3.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script4.csx")))
                           .Returns(_file4.ToArray());

                _fileSystem.Setup(fs => fs.GetFullPath(It.IsAny<string>())).Returns<string>((path) => path);
            }

            [Fact]
            public void MultipleUsingStatementsShouldProduceDistinctOutput()
            {
                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i.StartsWith("script"))), Times.Exactly(3));
                result.Namespaces.Count.ShouldEqual(2);
            }

            [Fact]
            public void UsingStateMentsShoulAllBeAtTheTop()
            {
                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                var splitOutput = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var lastUsing = splitOutput.ToList().FindLastIndex(x => x.TrimStart(' ').StartsWith("using "));
                var firsNotUsing = splitOutput.ToList().FindIndex(x => !x.TrimStart(' ').StartsWith("using "));

                Assert.True(lastUsing < firsNotUsing);
            }

            [Fact]
            public void ShouldNotLoadInlineLoads()
            {
                var processor = GetFilePreProcessor();
                processor.ProcessFile("script1.csx");

                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script1.csx")), Times.Once());
                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script2.csx")), Times.Once());
                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script3.csx")), Times.Never());
                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script4.csx")), Times.Once());
            }

            [Fact]
            public void ShouldReturnResultWithAllLoadedFiles()
            {
                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.LoadedScripts.Count.ShouldEqual(3);
                result.LoadedScripts.ShouldContain("script1.csx");
                result.LoadedScripts.ShouldContain("script2.csx");
                result.LoadedScripts.ShouldContain("script4.csx");
            }

            [Fact]
            public void ShouldReturnResultWithAllUsings()
            {
                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.Namespaces.Count.ShouldEqual(2);
                result.Namespaces.ShouldContain("System");
                result.Namespaces.ShouldContain("System.Core");
            }

            [Fact]
            public void ShouldReturnResultWithAllReferences()
            {
                var file1 = new List<string>
                    {
                        @"#r ""My.dll""",
                        @"#load ""scriptX.csx""",
                        "using System;",
                        @"Console.WriteLine(""Hi!"");"
                    };

                var file2 = new List<string>
                    {
                        @"#r ""My2.dll""",
                        "using System;",
                        @"Console.WriteLine(""Hi!"");"
                    };

                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx"))).Returns(file1.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "scriptX.csx"))).Returns(file2.ToArray());

                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.References.Count.ShouldEqual(2);
                result.References.ShouldContain("My.dll");
                result.References.ShouldContain("My2.dll");
            }

            [Fact]
            public void ShouldNotIncludeReferencesInCode()
            {
                var file1 = new List<string>
                    {
                        @"#r ""My.dll""",
                        @"#load ""scriptX.csx""",
                        "using System;",
                        @"Console.WriteLine(""Hi!"");"
                    };

                var file2 = new List<string>
                    {
                        @"#r ""My2.dll""",
                        "using System;",
                        @"Console.WriteLine(""Hi!"");"
                    };

                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx"))).Returns(file1.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "scriptX.csx"))).Returns(file2.ToArray());

                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.Code.ShouldNotContain("#r");
            }

            [Fact]
            public void ShouldNotIncludeShebangsInCode()
            {
                var file1 = new List<string>
                    {
                        @"#!/usr/bin/env scriptcs",
                        "using System;",
                        @"Console.WriteLine(""Hi!"");"
                    };

                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx"))).Returns(file1.ToArray());

                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.Code.ShouldNotContain("#!/usr/bin/env");
            }

            [Fact]
            public void ShouldNotLoadSameFileTwice()
            {
                var file = new List<string>
                    {
                        @"#load ""script4.csx""",
                        "using System;",
                        @"Console.WriteLine(""Hello Script 2"");",
                    };

                var fs = new Mock<IFileSystem>();
                fs.Setup(i => i.NewLine).Returns(Environment.NewLine);
                fs.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script2.csx")))
                  .Returns(file.ToArray());
                fs.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script4.csx")))
                  .Returns(_file4.ToArray());

                var processor = GetFilePreProcessor();
                processor.ProcessFile("script1.csx");

                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script1.csx")), Times.Once());
                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script2.csx")), Times.Once());
                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script3.csx")), Times.Never());
                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script4.csx")), Times.Once());
            }

            [Fact]
            public void LoadBeforeUsingShouldBeAllowed()
            {
                var file = new List<string>
                    {
                        @"#load ""script4.csx""",
                        string.Empty,
                        "using System;",
                        @"Console.WriteLine(""abc"");"
                    };

                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "file.csx"))).Returns(file.ToArray());

                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("file.csx");

                var splitOutput = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var lastUsing = splitOutput.ToList().FindLastIndex(x => x.TrimStart(' ').StartsWith("using "));
                var firsNotUsing = splitOutput.ToList().FindIndex(x => !x.TrimStart(' ').StartsWith("using "));

                splitOutput.Count(x => x.TrimStart(' ').StartsWith("using ")).ShouldEqual(0);
                result.Namespaces.ShouldContain("System");
                Assert.True(lastUsing < firsNotUsing);
            }

            [Fact]
            public void ShouldNotThrowStackOverflowExceptionOnLoadLoop()
            {
                var a = new List<string> { "#load B.csx" };
                var b = new List<string> { "#load A.csx" };

                _fileSystem.Setup(x => x.ReadFileLines("A.csx")).Returns(a.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines("B.csx")).Returns(b.ToArray());

                Assert.DoesNotThrow(() => GetFilePreProcessor().ProcessFile("A.csx"));
            }

            [Fact]
            public void ShouldNotBeAllowedToLoadAfterUsing()
            {
                var file = new List<string>
                    {
                        "using System;",
                        @"Console.WriteLine(""abc"");",
                        @"#load ""script4.csx"""
                    };

                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "file.csx"))).Returns(file.ToArray());

                var processor = GetFilePreProcessor();
                Assert.Throws(typeof(InvalidDirectiveUseException), () => processor.ProcessFile("file.csx"));

                _fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script4.csx")), Times.Never());
            }

            [Fact]
            public void UsingInCodeDoesNotCountAsUsingImport()
            {
                var file = new List<string>
                    {
                        @"#load ""script4.csx""",
                        string.Empty,
                        "using System;",
                        "using System.IO;",
                        "Console.WriteLine();",
                        @"using (var stream = new MemoryStream) {",
                        @"//do stuff",
                        @"}"
                    };
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "file.csx"))).Returns(file.ToArray());

                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("file.csx");

                var splitOutput = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var firstNonImportUsing =
                    splitOutput.ToList().FindIndex(x => x.TrimStart(' ').StartsWith("using ") && !x.Contains(";"));
                var firstCodeLine = splitOutput.ToList().FindIndex(x => x.Contains("Console"));

                Assert.True(firstNonImportUsing > firstCodeLine);
            }

            [Fact]
            public void ShouldHaveReferencesFromAllFiles()
            {
                var file1 = new List<string>
                    {
                        @"#r ""My.dll""",
                        @"#load ""scriptX.csx""",
                        "using System;",
                        @"Console.WriteLine(""Hi!"");"
                    };

                var file2 = new List<string>
                    {
                        @"#r ""My2.dll""",
                        "using System;",
                        @"Console.WriteLine(""Hi!"");"
                    };

                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx")))
                           .Returns(file1.ToArray());
                _fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "scriptX.csx")))
                           .Returns(file2.ToArray());

                var processor = GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.References.Count.ShouldEqual(2);
            }

            [Fact]
            public void ShouldAddLineDirectiveRightAfterLastLoadIsIncludedInEachFile()
            {
                // f1 has usings and then loads
                var f1 = new List<string>
                        {
                            @"#load ""C:\f2.csx"";",
                            @"#load ""C:\f3.csx"";",
                            "using System;",
                            "using System.Diagnostics;",
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

                var preProcessor = GetFilePreProcessor();

                var result = preProcessor.ProcessFile(@"C:\f1.csx");

                var fileLines = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // using should not be here, so only f4 -> f5 -> f2 -> f3 -> f1
                var line = 0;

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

                result.Namespaces.Count.ShouldEqual(2);
            }

            [Fact]
            public void ShouldLoadNestedScriptcsRelativeToScriptLocation()
            {
                // f1 has usings and then loads
                var f1 = new List<string>
                    {
                        @"#load ""SubFolder\f2.csx"";",
                        @"#load ""SubFolder\f3.csx"";",
                        @"using System;",
                        @"Console.WriteLine(""First line of f1"");"
                    };

                // f2 has no usings and multiple loads
                var f2 = new List<string>
                    {
                        @"#load ""f3.csx"";",
                        @"using System;",
                        @"Console.WriteLine(""First line of f2"");"
                    };

                // f3 has usings and no loads
                var f3 = new List<string>
                    {
                        @"using System;",
                        @"Console.WriteLine(""First line of f3"");"
                    };

                var currentDirectory = "c:\\";
                _fileSystem.SetupGet(y => y.CurrentDirectory).Returns(() => currentDirectory);
                _fileSystem.SetupSet(fs => fs.CurrentDirectory = It.IsAny<string>())
                           .Callback<string>((newCurrentDirectory) =>
                           {
                               currentDirectory = newCurrentDirectory;
                           });

                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f1.csx"))
                    .Returns(f1.ToArray());
                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\SubFolder\f2.csx"))
                    .Returns(f2.ToArray());
                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\SubFolder\f3.csx"))
                    .Returns(f3.ToArray());

                _fileSystem.Setup(fs => fs.GetFullPath(@"C:\f1.csx")).Returns(@"C:\f1.csx");
                _fileSystem.Setup(fs => fs.GetFullPath(@"SubFolder\f2.csx")).Returns(@"C:\SubFolder\f2.csx");
                _fileSystem.Setup(fs => fs.GetFullPath(@"f3.csx")).Returns(@"C:\SubFolder\f3.csx");
                _fileSystem.Setup(fs => fs.GetFullPath(@"SubFolder\f3.csx")).Returns(@"C:\SubFolder\f3.csx");

                _fileSystem.Setup(fs => fs.GetWorkingDirectory(@"C:\f1.csx")).Returns(@"C:\");
                _fileSystem.Setup(fs => fs.GetWorkingDirectory(@"C:\SubFolder\f2.csx")).Returns(@"C:\SubFolder\");
                _fileSystem.Setup(fs => fs.GetWorkingDirectory(@"C:\SubFolder\f3.csx")).Returns(@"C:\SubFolder\");

                var preProcessor = GetFilePreProcessor();

                preProcessor.ProcessFile(@"C:\f1.csx");

                _fileSystem.Verify(fs => fs.ReadFileLines(@"C:\f1.csx"), Times.Once());
                _fileSystem.Verify(fs => fs.ReadFileLines(@"C:\SubFolder\f2.csx"), Times.Once());
                _fileSystem.Verify(fs => fs.ReadFileLines(@"C:\SubFolder\f3.csx"), Times.Once());
            }

            [Fact]
            public void ShouldResetTheCurrentDirectoryWhenLoadingScript()
            {
                // f1 has usings and then loads
                var f1 = new List<string>
                    {
                        @"using System;",
                        @"Console.WriteLine(""First line of f1"");"
                    };

                var startingDirectory = "c:\\";
                var currentDirectory = startingDirectory;
                var lastCurrentDirectory = string.Empty;
                _fileSystem.SetupGet(y => y.CurrentDirectory).Returns(() => currentDirectory);
                _fileSystem.SetupSet(fs => fs.CurrentDirectory = It.IsAny<string>())
                           .Callback<string>((newCurrentDirectory) =>
                           {
                               lastCurrentDirectory = newCurrentDirectory;
                               currentDirectory = newCurrentDirectory;
                           });

                _fileSystem.Setup(fs => fs.ReadFileLines(@"C:\SubFolder\f1.csx"))
                    .Returns(f1.ToArray());
                _fileSystem.Setup(fs => fs.GetFullPath(@"C:\SubFolder\f1.csx")).Returns(@"C:\SubFolder\f1.csx");
                _fileSystem.Setup(fs => fs.GetWorkingDirectory(@"C:\SubFolder\f1.csx")).Returns(@"C:\SubFolder\");

                var preProcessor = GetFilePreProcessor();

                preProcessor.ProcessFile(@"C:\f1.csx");

                lastCurrentDirectory.ShouldBeSameAs(startingDirectory);
            }

            private IFilePreProcessor GetFilePreProcessor()
            {
                var lineProcessors = new ILineProcessor[]
                {
                    new UsingLineProcessor(),
                    new ReferenceLineProcessor(_fileSystem.Object),
                    new LoadLineProcessor(_fileSystem.Object),
                    new ShebangLineProcessor()
                };

                return new FilePreProcessor(_fileSystem.Object, new TestLogProvider(), lineProcessors);
            }
        }

        public class ProcessScriptMethod
        {
            private readonly Mock<IFileSystem> _fileSystem;

            public ProcessScriptMethod()
            {
                _fileSystem = new Mock<IFileSystem>();
                _fileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
                _fileSystem.Setup(fs => fs.GetFullPath(It.IsAny<string>())).Returns<string>((path) => path);
            }

            [Fact]
            public void ShouldSplitScriptIntoLines()
            {
                var preProcessor = GetFilePreProcessor();
                var script = @"Console.WriteLine(""Testing..."");";

                preProcessor.ProcessScript(script);

                _fileSystem.Verify(x => x.SplitLines(script), Times.Once());
            }

            [Fact]
            public void ShouldNotReadFromFile()
            {
                var preProcessor = GetFilePreProcessor();
                var script = @"Console.WriteLine(""Testing..."");";

                preProcessor.ProcessScript(script);

                _fileSystem.Verify(x => x.ReadFileLines(It.IsAny<string>()), Times.Never());
            }

            [Fact]
            public void ShouldNotIncludeLineDirectiveForRootScript()
            {
                var script1 = new List<string> { @"Console.WriteLine(""Hello from script1.csx""" };
                _fileSystem.Setup(x => x.ReadFileLines("script1.csx")).Returns(script1.ToArray());
                _fileSystem.Setup(x => x.SplitLines(It.IsAny<string>()))
                    .Returns<string>(x => x.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

                var preProcessor = GetFilePreProcessor();
                var script = @"#load script1.csx";

                var result = preProcessor.ProcessScript(script);
                var fileLines = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                fileLines.Count(x => x.StartsWith("#line ")).ShouldEqual(1);
            }

            private IFilePreProcessor GetFilePreProcessor()
            {
                var lineProcessors = new ILineProcessor[]
                {
                    new UsingLineProcessor(),
                    new ReferenceLineProcessor(_fileSystem.Object),
                    new LoadLineProcessor(_fileSystem.Object),
                    new ShebangLineProcessor()
                };

                return new FilePreProcessor(_fileSystem.Object, new TestLogProvider(), lineProcessors);
            }
        }

        public class TheParseScriptMethod
        {
            private readonly Mock<IFileSystem> _fileSystem;

            public TheParseScriptMethod()
            {
                _fileSystem = new Mock<IFileSystem>();
                _fileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
                _fileSystem.Setup(fs => fs.GetFullPath(It.IsAny<string>())).Returns<string>((path) => path);
            }

            [Fact]
            public void ShouldProcessCustomDirectiveWhenItComesBeforeCode()
            {
                var testableDirectiveProcessor = new DirectiveLineProcessorTests.TestableDirectiveLineProcessor();
                var filePreprocessor = GetFilePreProcessor(testableDirectiveProcessor, new LoadLineProcessor(_fileSystem.Object));
                var lines = new List<string>
                    {
                        "#Test something",
                        "Console.WriteLine(\"Success\");"
                    };

                filePreprocessor.ParseScript(lines, new FileParserContext());
                testableDirectiveProcessor.InheritedProcessLineCalled.ShouldBeTrue();
            }

            [Fact]
            public void ShouldProcessALoadDirectiveWhenItComesAfterACustomDirective()
            {
                var testableDirectiveProcessor = new DirectiveLineProcessorTests.TestableDirectiveLineProcessor();
                var loadLineProcessor = new TestableLoadLineProcessor(_fileSystem.Object);
                var filePreprocessor = GetFilePreProcessor(testableDirectiveProcessor, loadLineProcessor);
                var lines = new List<string>
                    {
                        "#Test something",
                        "#load myscript.csx",
                        "Console.WriteLine(\"Success\");"
                    };

                filePreprocessor.ParseScript(lines, new FileParserContext());
                loadLineProcessor.InheritedProcessLineCalled.ShouldBeTrue();
            }

            private IFilePreProcessor GetFilePreProcessor(ILineProcessor customDirectiveProcessor, ILineProcessor loadLineProcessor)
            {
                var lineProcessors = new ILineProcessor[]
                    {
                        new UsingLineProcessor(),
                        new ReferenceLineProcessor(_fileSystem.Object),
                        loadLineProcessor,
                        new ShebangLineProcessor(),
                        customDirectiveProcessor
                    };

                return new FilePreProcessor(_fileSystem.Object, new TestLogProvider(), lineProcessors);
            }

            public class TestableLoadLineProcessor : LoadLineProcessor
            {
                public TestableLoadLineProcessor(IFileSystem fileSystem)
                    : base(fileSystem)
                { }
                public bool InheritedProcessLineCalled { get; private set; }
                protected override bool ProcessLine(IFileParser parser, FileParserContext context, string line)
                {
                    InheritedProcessLineCalled = true;
                    return true;
                }
            }
        }
    }
}