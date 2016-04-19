using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Moq.Protected;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class ReplTests
    {
        public class Mocks
        {
            public Mocks()
            {
                FileSystem = new Mock<IFileSystem>();
                FileSystem.SetupGet(x => x.CurrentDirectory).Returns("");
                FileSystem.SetupGet(x => x.BinFolder).Returns("bin");
                FileSystem.SetupGet(x => x.DllCacheFolder).Returns(".cache");
                FileSystem.SetupGet(x => x.PackagesFolder).Returns("scriptcs_packages");
                ScriptEngine = new Mock<IScriptEngine>();
                LogProvider = new TestLogProvider();
                ScriptLibraryComposer = new Mock<IScriptLibraryComposer>();
                ScriptLibraryComposer.SetupGet(p => p.ScriptLibrariesFile).Returns("PackageScripts.csx");
                Console = new Mock<IConsole>();
                ScriptPack = new Mock<IScriptPack>();
                FilePreProcessor = new Mock<IFilePreProcessor>();
                ObjectSerializer = new Mock<IObjectSerializer>();
                ReplCommands = new[] { new Mock<IReplCommand>() };
            }

            public Mock<IObjectSerializer> ObjectSerializer { get; private set; }

            public Mock<IFileSystem> FileSystem { get; private set; }

            public Mock<IScriptEngine> ScriptEngine { get; private set; }

            public TestLogProvider LogProvider { get; private set; }

            public Mock<IConsole> Console { get; private set; }

            public Mock<IScriptPack> ScriptPack { get; private set; }

            public Mock<IFilePreProcessor> FilePreProcessor { get; private set; }

            public Mock<IScriptLibraryComposer> ScriptLibraryComposer { get; private set; }

            public Mock<IReplCommand>[] ReplCommands { get; set; }
        }

        public static Repl GetRepl(Mocks mocks)
        {
            return new Repl(
                new string[0],
                mocks.FileSystem.Object,
                mocks.ScriptEngine.Object,
                mocks.ObjectSerializer.Object,
                mocks.LogProvider,
                mocks.ScriptLibraryComposer.Object,
                mocks.Console.Object,
                mocks.FilePreProcessor.Object,
                mocks.ReplCommands.Select(x => x.Object),
                new Printers(mocks.ObjectSerializer.Object));
        }

        public class TheConstructor
        {
            [Fact]
            public void ShouldProperlyInitializeMembers()
            {
                var mocks = new Mocks();
                var repl = GetRepl(mocks);
                repl.FileSystem.ShouldEqual(mocks.FileSystem.Object);
                repl.ScriptEngine.ShouldEqual(mocks.ScriptEngine.Object);
                repl.Console.ShouldEqual(mocks.Console.Object);
            }
        }

        public class TheInitializeMethod
        {
            private Mocks _mocks;
            private Repl _repl;
            private string _tempPath;

            public TheInitializeMethod()
            {
                _tempPath = Path.GetTempPath();

                _mocks = new Mocks();
                _repl = GetRepl(_mocks);
                _mocks.FileSystem.Setup(x => x.CurrentDirectory).Returns(_tempPath);
                var paths = new[] { Path.Combine(_tempPath, "path") };
                _repl.Initialize(paths, new[] { _mocks.ScriptPack.Object });
            }

            [Fact]
            public void PopulatesReferences()
            {
                foreach (var reference in Repl.DefaultReferences)
                {
                    _repl.References.Paths.ShouldContain(reference);
                }

                _repl.References.Paths.ShouldContain(Path.Combine(_tempPath, "path"));
            }

            [Fact]
            public void SetsTheScriptEngineBaseDirectory()
            {
                _mocks.ScriptEngine.VerifySet(x => x.BaseDirectory = Path.Combine(_tempPath, "bin"));
            }

            [Fact]
            public void CreatesTheScriptPackSession()
            {
                _repl.ScriptPackSession.ShouldNotBeNull();
            }

            [Fact]
            public void InitializesScriptPacks()
            {
                _mocks.ScriptPack.Verify(x => x.Initialize(It.IsAny<IScriptPackSession>()));
            }
        }

        public class TheTerminateMethod
        {
            private Mocks _mocks;
            private Repl _repl;

            public TheTerminateMethod()
            {
                _mocks = new Mocks();
                _repl = GetRepl(_mocks);
                _mocks.FileSystem.Setup(x => x.CurrentDirectory).Returns(@"c:\");
                _repl.Initialize(new List<string>(), new[] { _mocks.ScriptPack.Object });
                _repl.Terminate();
            }

            [Fact]
            public void TerminatesScriptPacks()
            {
                _mocks.ScriptPack.Verify(x => x.Terminate());
            }

            [Fact]
            public void ExitsTheConsole()
            {
                _mocks.Console.Verify(x => x.Exit());
            }
        }

        public class TheExecuteMethod
        {
            private Mocks _mocks;
            private Repl _repl;

            public TheExecuteMethod()
            {
                _mocks = new Mocks();
                _mocks.FilePreProcessor.Setup(x => x.ProcessScript("foo"))
                    .Returns(new FilePreProcessorResult { Code = "foo" });
                _repl = GetRepl(_mocks);
                _repl.Console.ForegroundColor = ConsoleColor.White;
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>(), String.Empty);
                _repl.Execute("foo");
            }

            [Fact]
            public void SetsTheForegroundColorToCyan()
            {
                _mocks.Console.VerifySet(x => x.ForegroundColor = ConsoleColor.Cyan, Times.Once());
            }

            [Fact]
            public void ResetsColorAfterExecutingScript()
            {
                _mocks.Console.Verify(x => x.ResetColor());
            }

            [Fact]
            public void CallsExecuteOnTheScriptEngine()
            {
                _mocks.ScriptEngine.Verify(
                    x => x.Execute(
                        "foo",
                        new string[0],
                        It.Is<AssemblyReferences>(i => i.Assemblies.SequenceEqual(_repl.References.Assemblies)),
                        It.Is<IEnumerable<string>>(i => i.SequenceEqual(_repl.Namespaces)),
                        It.IsAny<ScriptPackSession>()));
            }

            [Fact]
            public void ShouldPassExtraNameSpacesToEngineIfFilePreProcessorProducesThem()
            {
                _mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.Is<string>(i => i == "#load foo.csx")))
                    .Returns(new FilePreProcessorResult { Namespaces = new List<string> { "Foo", "Bar" } });

                _repl.Execute("#load foo.csx");

                _mocks.ScriptEngine.Verify(x => x.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.Is<IEnumerable<string>>(i => i.SequenceEqual(_repl.Namespaces)),
                    It.IsAny<ScriptPackSession>()));

                _repl.Namespaces.Count().ShouldEqual(ScriptExecutor.DefaultNamespaces.Count() + 2);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddNamespacesFromScriptLibrary(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                Mock<Repl> repl
                )
            {
                repl.Protected();
                repl.Setup(r => r.InjectScriptLibraries(It.IsAny<string>(), It.IsAny<FilePreProcessorResult>(), It.IsAny<IDictionary<string, object>>()))
                    .Callback((string p, FilePreProcessorResult r, IDictionary<string, object> s) =>
                    {
                        r.Namespaces.Add("Foo.Bar");
                    });

                scriptEngine.Setup(e => e.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));

                repl.Object.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                repl.Object.Execute("", new string[] {});

                scriptEngine.Verify(
                    e => e.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.Is<IEnumerable<string>>(x => x.Contains("Foo.Bar")),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddReferencesFromScriptLibrary(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                Mock<Repl> repl
                )
            {
                repl.Protected();
                repl.Setup(r => r.InjectScriptLibraries(It.IsAny<string>(), It.IsAny<FilePreProcessorResult>(), It.IsAny<IDictionary<string, object>>()))
                    .Callback((string p, FilePreProcessorResult r, IDictionary<string, object> s) =>
                    {
                        r.References.Add("Foo.Bar");
                    });

                scriptEngine.Setup(e => e.Execute(
                    It.IsAny<string>(),
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));


                repl.Object.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                repl.Object.Execute("", new string[] { });

                scriptEngine.Verify(
                    e => e.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.Is<AssemblyReferences>(x => x.Paths.Contains("Foo.Bar")),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Fact]
            public void ShouldRemoveInvalidNamespacesIfScriptResultContainsany()
            {
                _mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.Is<string>(i => i == "#load foo.csx")))
                    .Returns(new FilePreProcessorResult { Namespaces = new List<string> { "Foo", "Bar" } });
                _mocks.ScriptEngine.Setup(
                    x => x.Execute(
                            It.IsAny<string>(),
                            It.IsAny<string[]>(),
                            It.IsAny<AssemblyReferences>(),
                            It.IsAny<IEnumerable<string>>(),
                            It.IsAny<ScriptPackSession>())).Returns(new ScriptResult(invalidNamespaces: new string[] { "Foo" }));

                _repl.Execute("#load foo.csx");
                _repl.Namespaces.Count().ShouldEqual(ScriptExecutor.DefaultNamespaces.Count() + 1);
                _repl.Namespaces.ShouldNotContain("Foo");
            }

            [Fact]
            public void CatchesExceptionsAndWritesThemInRed()
            {
                _mocks.ScriptEngine.Setup(
                    x => x.Execute(
                            It.IsAny<string>(),
                            It.IsAny<string[]>(),
                            It.IsAny<AssemblyReferences>(),
                            It.IsAny<IEnumerable<string>>(),
                            It.IsAny<ScriptPackSession>()))
                        .Throws<ArgumentException>();

                _repl.Execute("foo");

                _mocks.Console.VerifySet(x => x.ForegroundColor = ConsoleColor.Red);
                _mocks.Console.Verify(x => x.WriteLine(It.IsAny<string>()));
            }

            [Fact]
            public void SetsTheForegroundColorBackToTheDefault()
            {
                _mocks.Console.VerifySet(x => x.ForegroundColor = ConsoleColor.White);
            }

            [Fact]
            public void ShouldProcessFileIfLineIsALoad()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(x => x.FileExists("file.csx")).Returns(true);

                _repl = GetRepl(mocks);
                _repl.Execute("#load \"file.csx\"");

                mocks.FilePreProcessor.Verify(i => i.ProcessScript("#load \"file.csx\""), Times.Once());
            }

            [Fact]
            public void ShouldExecuteLoadedFileIfLineIsALoad()
            {
                var mocks = new Mocks();
                mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.IsAny<string>()))
                     .Returns(new FilePreProcessorResult());
                mocks.FileSystem.Setup(x => x.FileExists("file.csx")).Returns(true);
                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>(), "");
                _repl.Execute("#load \"file.csx\"");

                mocks.ScriptEngine.Verify(
                    i => i.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Once());
            }

            [Fact]
            public void ShouldNotExecuteLoadedFileIfFileDoesNotExist()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(x => x.FileExists("file.csx")).Returns(false);

                _repl = GetRepl(mocks);
                _repl.Execute("#load \"file.csx\"");

                mocks.ScriptEngine.Verify(
                    i => i.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Never());
            }

            [Fact]
            public void ShouldReferenceAssemblyIfLineIsAReference()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                mocks.FileSystem.Setup(i => i.GetFullPath(It.IsAny<string>())).Returns(@"c:/my.dll");
                mocks.FileSystem.Setup(x => x.FileExists("c:/my.dll")).Returns(true);
                mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { References = new List<string> { "my.dll" } });

                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("#r \"my.dll\"");

                //default references = 10, + 1 we just added
                _repl.References.Paths.Count().ShouldEqual(11);
            }

            [Fact]
            public void ShouldReferenceAssemblyBasedOnFullPathIfFileExists()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                mocks.FileSystem.Setup(i => i.GetFullPath(It.IsAny<string>())).Returns(@"C:/my.dll");
                mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { References = new List<string> { "my.dll" } });

                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("#r \"my.dll\"");

                mocks.FileSystem.Verify(x => x.FileExists("C:/my.dll"), Times.Once());
            }

            [Fact]
            public void ShouldReferenceAssemblyBasedOnNameIfFileDoesNotExistBecauseItLooksInGACThen()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                mocks.FileSystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);
                mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { References = new List<string> { "PresentationCore" } });

                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("#r \"PresentationCore\"");

                _repl.References.Paths.Contains("PresentationCore").ShouldBeTrue();
            }

            [Fact]
            public void ShouldReferenceAssemblyBasedOnNameWithExtensionIfFileDoesNotExistBecauseItLooksInGACThen()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                mocks.FileSystem.Setup(i => i.GetFullPath(It.IsAny<string>())).Returns(@"C:/my.dll");
                mocks.FileSystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);
                mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { References = new List<string> { "my.dll" } });

                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("#r \"my.dll\"");

                _repl.References.Paths.Contains("my.dll").ShouldBeTrue();
            }

            [Fact]
            public void ShouldRemoveReferenceIfAssemblyIsNotFound()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                mocks.FileSystem.Setup(i => i.GetFullPath(It.IsAny<string>())).Returns(@"C:/my.dll");
                mocks.FileSystem.Setup(i => i.FileExists(It.IsAny<string>())).Returns(false);
                mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { References = new List<string> { "my.dll" } });
                mocks.ScriptEngine.Setup(
                    i =>
                    i.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<AssemblyReferences>(),
                              It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()))
                     .Throws(new FileNotFoundException("error", "my.dll"));

                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("#r \"my.dll\"");
                _repl.References.Paths.Contains("my.dll").ShouldBeFalse();
            }

            [Fact]
            public void ShouldNotExecuteAnythingIfLineIsAReference()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("#r \"my.dll\"");

                mocks.ScriptEngine.Verify(
                    i => i.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Never());
            }

            [Fact]
            public void ShouldSetBufferIFLineIsFirstOfMultilineConstruct()
            {
                var mocks = new Mocks();
                mocks.ScriptEngine.Setup(
                    x =>
                    x.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<AssemblyReferences>(),
                              It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()))
                     .Returns<ScriptResult>(x => ScriptResult.Incomplete);
                mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { Code = "var x = 1;" });
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("var x = 1;");

                _repl.Buffer.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldResetBufferIfLineIsNoLongerMultilineConstruct()
            {
                var mocks = new Mocks();
                mocks.ScriptEngine.Setup(
                    x =>
                    x.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<AssemblyReferences>(),
                              It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()))
                     .Returns(ScriptResult.Empty);
                mocks.FilePreProcessor.Setup(x => x.ProcessScript(It.IsAny<string>()))
                    .Returns(new FilePreProcessorResult { Code = "}" });
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                _repl = GetRepl(mocks);
                _repl.Buffer = "class test {";
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("}");

                _repl.Buffer.ShouldBeNull();
            }

            [Fact]
            public void ShouldResubmitEverytingIfLineIsNoLongerMultilineConstruct()
            {
                var mocks = new Mocks();
                mocks.ScriptEngine.Setup(x => x.Execute(
                        It.Is<string>(i => i == "class test {}"),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()))
                    .Returns(ScriptResult.Empty);

                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                _repl = GetRepl(mocks);
                _repl.Buffer = "class test {";
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Execute("}");

                mocks.ScriptEngine.Verify();
            }

            [Fact]
            public void ShouldPickReplCommandIfLineStartsWithColon()
            {
                var helloCommand = new Mock<IReplCommand>();
                helloCommand.SetupGet(x => x.CommandName).Returns("hello");

                var mocks = new Mocks { ReplCommands = new[] { helloCommand } };
                _repl = GetRepl(mocks);

                _repl.Execute(":hello", null);

                helloCommand.Verify(x => x.Execute(_repl, It.Is<object[]>(i => i.Length == 0)), Times.Once);
            }

            [Fact]
            public void ShouldEvaluateArgs()
            {
                var dummyObject = new DummyClass { Hello = "World" };
                var helloCommand = new Mock<IReplCommand>();
                helloCommand.SetupGet(x => x.CommandName).Returns("hello");

                var mocks = new Mocks { ReplCommands = new[] { helloCommand } };
                mocks.ScriptEngine.Setup(x => x.Execute(
                        "myObj",
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()))
                    .Returns(new ScriptResult(returnValue: dummyObject));

                mocks.ScriptEngine.Setup(x => x.Execute(
                        "100",
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()))
                    .Returns(new ScriptResult(returnValue: 100));

                _repl = GetRepl(mocks);

                _repl.Execute(":hello 100 myObj", null);

                helloCommand.Verify(
                    x => x.Execute(
                        _repl,
                        It.Is<object[]>(i =>
                            i[0].GetType() == typeof(int) &&
                            (int)i[0] == 100 &&
                            i[1].Equals(dummyObject))),
                    Times.Once);
            }

            [Fact]
            public void ShouldEvaluateStrings()
            {
                var helloCommand = new Mock<IReplCommand>();
                helloCommand.SetupGet(x => x.CommandName).Returns("hello");

                var mocks = new Mocks { ReplCommands = new[] { helloCommand } };
                mocks.ScriptEngine.Setup(x => x.Execute(
                        "\"world\"",
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()))
                    .Returns(new ScriptResult(returnValue: "world"));

                _repl = GetRepl(mocks);

                _repl.Execute(":hello \"world\"", null);

                helloCommand.Verify(
                    x => x.Execute(
                        _repl,
                        It.Is<object[]>(i =>
                            i[0].GetType() == typeof(string) &&
                            (string)i[0] == "world")),
                    Times.Once);
            }

            [Fact]
            public void ShouldSurfaceArgumentCompilationErrors()
            {
                var helloCommand = new Mock<IReplCommand>();
                helloCommand.SetupGet(x => x.CommandName).Returns("hello");

                var mocks = new Mocks { ReplCommands = new[] { helloCommand } };
                mocks.ScriptEngine.Setup(x => x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()))
                        .Returns(new ScriptResult(compilationException: new Exception()));

                _repl = GetRepl(mocks);

                var result = _repl.Execute(":hello foo", null);

                result.ExecuteExceptionInfo.SourceException.Message.ShouldContain(
                    "argument is not a valid expression: foo", StringComparison.OrdinalIgnoreCase);
            }

            [Fact]
            public void ShouldSurfaceArgumentExecutionErrors()
            {
                var helloCommand = new Mock<IReplCommand>();
                helloCommand.SetupGet(x => x.CommandName).Returns("hello");

                var mocks = new Mocks { ReplCommands = new[] { helloCommand } };
                mocks.ScriptEngine.Setup(x => x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()))
                        .Returns(new ScriptResult(executionException: new Exception()));

                _repl = GetRepl(mocks);

                var result = _repl.Execute(":hello foo", null);

                result.ExecuteExceptionInfo.SourceException.Message.ShouldContain(
                    "argument is not a valid expression: foo", StringComparison.OrdinalIgnoreCase);
            }

            [Fact]
            public void ShouldSurfaceIncompleteArguments()
            {
                var helloCommand = new Mock<IReplCommand>();
                helloCommand.SetupGet(x => x.CommandName).Returns("hello");

                var mocks = new Mocks { ReplCommands = new[] { helloCommand } };
                mocks.ScriptEngine.Setup(x => x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()))
                        .Returns(ScriptResult.Incomplete);

                _repl = GetRepl(mocks);

                var result = _repl.Execute(":hello foo", null);

                result.ExecuteExceptionInfo.SourceException.Message.ShouldContain(
                    "argument is not a valid expression: foo", StringComparison.OrdinalIgnoreCase);
            }

            [Fact]
            public void ShouldPrintTheReturnToConsoleIfCommandHasReturnValue()
            {
                object returnValue = new DummyClass { Hello = "World" };

                var helloCommand = new Mock<IReplCommand>();
                helloCommand.SetupGet(x => x.CommandName).Returns("hello");
                helloCommand.Setup(x => x.Execute(It.IsAny<IRepl>(), It.IsAny<object[]>()))
                    .Returns(returnValue);

                var mocks = new Mocks { ReplCommands = new[] { helloCommand } };
                mocks.ObjectSerializer.Setup(x => x.Serialize(returnValue)).Returns("hello world");

                _repl = GetRepl(mocks);

                _repl.Execute(":hello", null);

                mocks.ObjectSerializer.Verify(x => x.Serialize(returnValue), Times.Once);
                mocks.Console.Verify(x => x.WriteLine("hello world"), Times.Once);
            }

            [Fact]
            public void ShouldReturnCommandsScriptResultfCommandHasReturnValueThatAlreadyIsScriptResult()
            {
                var returnValue = new ScriptResult("hello world");

                var helloCommand = new Mock<IReplCommand>();
                helloCommand.SetupGet(x => x.CommandName).Returns("hello");
                helloCommand.Setup(x => x.Execute(It.IsAny<IRepl>(), It.IsAny<object[]>()))
                    .Returns(returnValue);

                var mocks = new Mocks { ReplCommands = new[] { helloCommand } };
                mocks.ObjectSerializer.Setup(x => x.Serialize(returnValue.ReturnValue)).Returns("hello world");

                _repl = GetRepl(mocks);

                var result = _repl.Execute(":hello", null);

                mocks.ObjectSerializer.Verify(x => x.Serialize(returnValue.ReturnValue), Times.Once);
                mocks.Console.Verify(x => x.WriteLine("hello world"), Times.Once);
                result.ShouldBeSameAs(returnValue);
            }
        }

        private class DummyClass
        {
            public string Hello { get; set; }
        }
    }
}
