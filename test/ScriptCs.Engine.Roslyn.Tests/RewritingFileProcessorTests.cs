namespace ScriptCs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Common.Logging;

    using Moq;

    using Roslyn.Compilers.CSharp;

    using ScriptCs.Contracts;
    using ScriptCs.Contracts.Exceptions;

    using Should;

    using Xunit;

    public class RewritingFileProcessorTests
    {
        public class ProcessFileMethod
        {
            private List<string> _file1 = new List<string>
                                              {
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

            private List<string> _file5 = new List<string>
                                              {
                                                  "using System;",
                                                  @"Console.WriteLine(""Hello Script 2"");",
                                                  @"Console.WriteLine(""Loading Script 3"");",
                                                  @"#load ""script3.csx""",
                                                  @"Console.WriteLine(""Loaded Script 3"");",
                                                  @"Console.WriteLine(""Goodbye Script 2"");"
                                              };

            private readonly Mock<IFileSystem> _fileSystem;

            public ProcessFileMethod()
            {
                this._fileSystem = new Mock<IFileSystem>();
                this._fileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx")))
                    .Returns(this._file1.ToArray());
                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script2.csx")))
                    .Returns(this._file2.ToArray());
                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script3.csx")))
                    .Returns(this._file3.ToArray());
                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script4.csx")))
                    .Returns(this._file4.ToArray());

                this._fileSystem.Setup(fs => fs.GetFullPath(It.IsAny<string>())).Returns<string>((path) => path);
            }

            [Fact]
            public void RewritesScript()
            {
                var file1 = new[] { "using System;", "Console.WriteLine(\"Hi!\");" };

                this._fileSystem.Setup(x => x.ReadFileLines(It.IsAny<string>())).Returns(file1);

                var processor = this.GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                Assert.Equal(@"using System;

#line 2 ""script1.csx""
Console.WriteLine(@""blah"");", result.Code);
            }

            [Fact]
            public void MultipleUsingStatementsShouldProduceDistinctOutput()
            {
                var processor = this.GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                var splitOutput = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                this._fileSystem.Verify(
                    x => x.ReadFileLines(It.Is<string>(i => i.StartsWith("script"))),
                    Times.Exactly(3));
                Assert.Equal(2, splitOutput.Count(x => x.TrimStart(' ').StartsWith("using ")));
            }

            [Fact]
            public void UsingStateMentsShoulAllBeAtTheTop()
            {
                var processor = this.GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                var splitOutput = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var lastUsing = splitOutput.ToList().FindLastIndex(x => x.TrimStart(' ').StartsWith("using "));
                var firsNotUsing = splitOutput.ToList().FindIndex(x => !x.TrimStart(' ').StartsWith("using "));

                Assert.True(lastUsing < firsNotUsing);
            }

            [Fact]
            public void ShouldNotLoadInlineLoads()
            {
                var processor = this.GetFilePreProcessor();
                processor.ProcessFile("script1.csx");

                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script1.csx")), Times.Once());
                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script2.csx")), Times.Once());
                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script3.csx")), Times.Never());
                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script4.csx")), Times.Once());
            }

            [Fact]
            public void ShouldReturnResultWithAllLoadedFiles()
            {
                var processor = this.GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.LoadedScripts.Count.ShouldEqual(3);
                result.LoadedScripts.ShouldContain("script1.csx");
                result.LoadedScripts.ShouldContain("script2.csx");
                result.LoadedScripts.ShouldContain("script4.csx");
            }

            [Fact]
            public void ShouldReturnResultWithAllUsings()
            {
                var processor = this.GetFilePreProcessor();
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

                var file2 = new List<string> { @"#r ""My2.dll""", "using System;", @"Console.WriteLine(""Hi!"");" };

                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx")))
                    .Returns(file1.ToArray());
                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "scriptX.csx")))
                    .Returns(file2.ToArray());

                var processor = this.GetFilePreProcessor();
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

                var file2 = new List<string> { @"#r ""My2.dll""", "using System;", @"Console.WriteLine(""Hi!"");" };

                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx")))
                    .Returns(file1.ToArray());
                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "scriptX.csx")))
                    .Returns(file2.ToArray());

                var processor = this.GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.Code.ShouldNotContain("#r");
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
                fs.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script2.csx"))).Returns(file.ToArray());
                fs.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script4.csx"))).Returns(this._file4.ToArray());

                var processor = this.GetFilePreProcessor();
                processor.ProcessFile("script1.csx");

                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script1.csx")), Times.Once());
                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script2.csx")), Times.Once());
                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script3.csx")), Times.Never());
                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script4.csx")), Times.Once());
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

                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "file.csx")))
                    .Returns(file.ToArray());

                var processor = this.GetFilePreProcessor();
                var result = processor.ProcessFile("file.csx");

                var splitOutput = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var lastUsing = splitOutput.ToList().FindLastIndex(x => x.TrimStart(' ').StartsWith("using "));
                var firsNotUsing = splitOutput.ToList().FindIndex(x => !x.TrimStart(' ').StartsWith("using "));

                splitOutput.Count(x => x.TrimStart(' ').StartsWith("using ")).ShouldEqual(2);
                Assert.True(lastUsing < firsNotUsing);
            }

            [Fact]
            public void ShouldNotThrowStackOverflowExceptionOnLoadLoop()
            {
                var a = new List<string> { "#load B.csx" };
                var b = new List<string> { "#load A.csx" };

                this._fileSystem.Setup(x => x.ReadFileLines("A.csx")).Returns(a.ToArray());
                this._fileSystem.Setup(x => x.ReadFileLines("B.csx")).Returns(b.ToArray());

                Assert.DoesNotThrow(() => this.GetFilePreProcessor().ProcessFile("A.csx"));
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

                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "file.csx")))
                    .Returns(file.ToArray());

                var processor = this.GetFilePreProcessor();
                Assert.Throws(typeof(InvalidDirectiveUseException), () => processor.ProcessFile("file.csx"));

                this._fileSystem.Verify(x => x.ReadFileLines(It.Is<string>(i => i == "script4.csx")), Times.Never());
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
                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "file.csx")))
                    .Returns(file.ToArray());

                var processor = this.GetFilePreProcessor();
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

                var file2 = new List<string> { @"#r ""My2.dll""", "using System;", @"Console.WriteLine(""Hi!"");" };

                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "script1.csx")))
                    .Returns(file1.ToArray());
                this._fileSystem.Setup(x => x.ReadFileLines(It.Is<string>(f => f == "scriptX.csx")))
                    .Returns(file2.ToArray());

                var processor = this.GetFilePreProcessor();
                var result = processor.ProcessFile("script1.csx");

                result.References.Count.ShouldEqual(2);
            }

            [Fact]
            public void ShouldAddLineDirectiveRightAfterLastLoadIsIncludedInEachFile()
            {
                var rewrittenInvocation = @"Console.WriteLine(@""blah"");";
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
                var f4 = new List<string> { @"Console.WriteLine(""First line of f4"");", };

                // f5 is no special case, just used to be loaded
                var f5 = new List<string> { @"using System;", @"Console.WriteLine(""First line of f5"");", };

                this._fileSystem.SetupGet(fs => fs.NewLine).Returns(Environment.NewLine);
                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f1.csx")).Returns(f1.ToArray());
                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f2.csx")).Returns(f2.ToArray()).Verifiable();
                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f3.csx")).Returns(f3.ToArray());
                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f4.csx")).Returns(f4.ToArray());
                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f5.csx")).Returns(f5.ToArray());
                this._fileSystem.Setup(fs => fs.IsPathRooted(It.IsAny<string>())).Returns(true);

                var preProcessor = this.GetFilePreProcessor();

                var result = preProcessor.ProcessFile(@"C:\f1.csx");

                var fileLines = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // using statements go first, after that f4 -> f5 -> f2 -> f3 -> f1
                var line = 0;
                fileLines[line++].ShouldEqual("using System;");
                fileLines[line++].ShouldEqual("using System.Diagnostics;");

                line++; // Skip blank separator line between usings and body...

                fileLines[line++].ShouldEqual(@"#line 1 ""C:\f4.csx""");
                fileLines[line++].ShouldEqual(rewrittenInvocation);

                fileLines[line++].ShouldEqual(@"#line 2 ""C:\f5.csx""");
                fileLines[line++].ShouldEqual(rewrittenInvocation);

                fileLines[line++].ShouldEqual(@"#line 3 ""C:\f2.csx""");
                fileLines[line++].ShouldEqual(rewrittenInvocation);

                fileLines[line++].ShouldEqual(@"#line 3 ""C:\f3.csx""");
                fileLines[line++].ShouldEqual(rewrittenInvocation);

                fileLines[line++].ShouldEqual(@"#line 5 ""C:\f1.csx""");
                fileLines[line].ShouldEqual(rewrittenInvocation);
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
                var f3 = new List<string> { @"using System;", @"Console.WriteLine(""First line of f3"");" };

                var currentDirectory = "c:\\";
                this._fileSystem.SetupGet(y => y.CurrentDirectory).Returns(() => currentDirectory);
                this._fileSystem.SetupSet(fs => fs.CurrentDirectory = It.IsAny<string>())
                    .Callback<string>(
                        (newCurrentDirectory) =>
                        {
                            currentDirectory = newCurrentDirectory;
                        });

                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\f1.csx")).Returns(f1.ToArray());
                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\SubFolder\f2.csx")).Returns(f2.ToArray());
                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\SubFolder\f3.csx")).Returns(f3.ToArray());

                this._fileSystem.Setup(fs => fs.GetFullPath(@"C:\f1.csx")).Returns(@"C:\f1.csx");
                this._fileSystem.Setup(fs => fs.GetFullPath(@"SubFolder\f2.csx")).Returns(@"C:\SubFolder\f2.csx");
                this._fileSystem.Setup(fs => fs.GetFullPath(@"f3.csx")).Returns(@"C:\SubFolder\f3.csx");
                this._fileSystem.Setup(fs => fs.GetFullPath(@"SubFolder\f3.csx")).Returns(@"C:\SubFolder\f3.csx");

                this._fileSystem.Setup(fs => fs.GetWorkingDirectory(@"C:\f1.csx")).Returns(@"C:\");
                this._fileSystem.Setup(fs => fs.GetWorkingDirectory(@"C:\SubFolder\f2.csx")).Returns(@"C:\SubFolder\");
                this._fileSystem.Setup(fs => fs.GetWorkingDirectory(@"C:\SubFolder\f3.csx")).Returns(@"C:\SubFolder\");

                var preProcessor = this.GetFilePreProcessor();

                var result = preProcessor.ProcessFile(@"C:\f1.csx");

                this._fileSystem.Verify(fs => fs.ReadFileLines(@"C:\f1.csx"), Times.Once());
                this._fileSystem.Verify(fs => fs.ReadFileLines(@"C:\SubFolder\f2.csx"), Times.Once());
                this._fileSystem.Verify(fs => fs.ReadFileLines(@"C:\SubFolder\f3.csx"), Times.Once());
            }

            [Fact]
            public void ShouldResetTheCurrentDirectoryWhenLoadingScript()
            {
                // f1 has usings and then loads
                var f1 = new List<string> { @"using System;", @"Console.WriteLine(""First line of f1"");" };

                var startingDirectory = "c:\\";
                var currentDirectory = startingDirectory;
                var lastCurrentDirectory = string.Empty;
                this._fileSystem.SetupGet(y => y.CurrentDirectory).Returns(() => currentDirectory);
                this._fileSystem.SetupSet(fs => fs.CurrentDirectory = It.IsAny<string>())
                    .Callback<string>(
                        (newCurrentDirectory) =>
                        {
                            lastCurrentDirectory = newCurrentDirectory;
                            currentDirectory = newCurrentDirectory;
                        });

                this._fileSystem.Setup(fs => fs.ReadFileLines(@"C:\SubFolder\f1.csx")).Returns(f1.ToArray());
                this._fileSystem.Setup(fs => fs.GetFullPath(@"C:\SubFolder\f1.csx")).Returns(@"C:\SubFolder\f1.csx");
                this._fileSystem.Setup(fs => fs.GetWorkingDirectory(@"C:\SubFolder\f1.csx")).Returns(@"C:\SubFolder\");

                var preProcessor = this.GetFilePreProcessor();

                var result = preProcessor.ProcessFile(@"C:\f1.csx");

                lastCurrentDirectory.ShouldBeSameAs(startingDirectory);
            }

            private IFilePreProcessor GetFilePreProcessor()
            {
                var rewriters = new[] { new StringRewriter() };
                var lineProcessors = new ILineProcessor[]
                                         {
                                             new UsingLineProcessor(), new ReferenceLineProcessor(this._fileSystem.Object),
                                             new LoadLineProcessor(this._fileSystem.Object)
                                         };

                return new RewritingFilePreProcessor(
                    this._fileSystem.Object,
                    Mock.Of<ILog>(),
                    lineProcessors,
                    rewriters);
            }
        }

        public class ProcessScriptMethod
        {
            private readonly Mock<IFileSystem> _fileSystem;

            public ProcessScriptMethod()
            {
                this._fileSystem = new Mock<IFileSystem>();
                this._fileSystem.Setup(x => x.SplitLines(It.IsAny<string>()))
                    .Returns<string>(x => x.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                this._fileSystem.SetupGet(x => x.NewLine).Returns(Environment.NewLine);
                this._fileSystem.Setup(fs => fs.GetFullPath(It.IsAny<string>())).Returns<string>((path) => path);
            }

            [Fact]
            public void ScriptIsRewritten()
            {
                var preProcessor = this.GetFilePreProcessor();
                var script = @"Console.WriteLine(""Testing..."");";

                var result = preProcessor.ProcessScript(script);

                Assert.Equal(@"Console.WriteLine(@""blah"");", result.Code);
            }

            [Fact]
            public void ShouldSplitScriptIntoLines()
            {
                var preProcessor = this.GetFilePreProcessor();
                var script = @"Console.WriteLine(""Testing..."");";

                preProcessor.ProcessScript(script);

                this._fileSystem.Verify(x => x.SplitLines(script), Times.Once());
            }

            [Fact]
            public void ShouldNotReadFromFile()
            {
                var preProcessor = this.GetFilePreProcessor();
                var script = @"Console.WriteLine(""Testing..."");";

                preProcessor.ProcessScript(script);

                this._fileSystem.Verify(x => x.ReadFileLines(It.IsAny<string>()), Times.Never());
            }

            [Fact]
            public void ShouldNotIncludeLineDirectiveForRootScript()
            {
                var script1 = new List<string> { @"Console.WriteLine(""Hello from script1.csx""" };
                this._fileSystem.Setup(x => x.ReadFileLines("script1.csx")).Returns(script1.ToArray());
                this._fileSystem.Setup(x => x.SplitLines(It.IsAny<string>()))
                    .Returns<string>(x => x.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

                var preProcessor = this.GetFilePreProcessor();
                var script = @"#load script1.csx";

                var result = preProcessor.ProcessScript(script);
                var fileLines = result.Code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                fileLines.Count(x => x.StartsWith("#line ")).ShouldEqual(1);
            }

            private IFilePreProcessor GetFilePreProcessor()
            {
                var rewriters = new[] { new StringRewriter() };
                var lineProcessors = new ILineProcessor[]
                                         {
                                             new UsingLineProcessor(), new ReferenceLineProcessor(this._fileSystem.Object),
                                             new LoadLineProcessor(this._fileSystem.Object)
                                         };

                return new RewritingFilePreProcessor(
                    this._fileSystem.Object,
                    Mock.Of<ILog>(),
                    lineProcessors,
                    rewriters);
            }
        }
    }
}