using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ReplTests
    {
        public class Mocks
        {
            public Mocks()
            {
                FileSystem = new Mock<IFileSystem>();
                ScriptEngine = new Mock<IScriptEngine>();
                Logger = new Mock<ILog>();
                Console = new Mock<IConsole>();
                ScriptPack = new Mock<IScriptPack>();
                FilePreProcessor = new Mock<IFilePreProcessor>();
            }

            public Mock<IFileSystem> FileSystem { get; private set; }
            public Mock<IScriptEngine> ScriptEngine { get; private set; }
            public Mock<ILog> Logger { get; private set; }
            public Mock<IConsole> Console { get; private set; }
            public Mock<IScriptPack> ScriptPack { get; private set; }
            public Mock<IFilePreProcessor> FilePreProcessor { get; private set; }
        }

        public static Repl GetRepl(Mocks mocks)
        {
            return new Repl(mocks.FileSystem.Object, mocks.ScriptEngine.Object, mocks.Logger.Object, mocks.Console.Object, mocks.FilePreProcessor.Object);
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
                repl.Logger.ShouldEqual(mocks.Logger.Object);
                repl.Console.ShouldEqual(mocks.Console.Object);
            }
        }

        public class TheInitializeMethod
        {
            private Mocks _mocks;
            private Repl _repl;
 
            public TheInitializeMethod()
            {
                _mocks = new Mocks();
                _repl = GetRepl(_mocks);
                _mocks.FileSystem.Setup(x => x.CurrentDirectory).Returns(@"c:\");
                var paths = new []{@"c:\path"};
                _repl.Initialize(paths, new[] {_mocks.ScriptPack.Object});
            }    

            [Fact]
            public void PopulatesReferences()
            {
                foreach (var reference in Repl.DefaultReferences)
                {
                    _repl.References.ShouldContain(reference);
                }
                _repl.References.ShouldContain(@"c:\path");
            }

            [Fact]
            public void SetsTheScriptEngineBaseDirectory()
            {
                _mocks.ScriptEngine.VerifySet(x=>x.BaseDirectory = @"c:\bin");   
            }

            [Fact]
            public void CreatesTheScriptPackSession()
            {
                _repl.ScriptPackSession.ShouldNotBeNull();
            }

            [Fact]
            public void InitializesScriptPacks()
            {
                _mocks.ScriptPack.Verify(x=>x.Initialize(It.IsAny<IScriptPackSession>()));
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
                _repl.Initialize(new List<string>(), new[] {_mocks.ScriptPack.Object});
                _repl.Terminate();
            }

            [Fact]
            public void TerminatesScriptPacks()
            {
                _mocks.ScriptPack.Verify(x => x.Terminate());
            }
        }

        public class TheExecuteMethod
        {
            private Mocks _mocks;
            private Repl _repl;
 
            public TheExecuteMethod()
            {
                _mocks = new Mocks();
                _repl = GetRepl(_mocks);
                _repl.Console.ForegroundColor = ConsoleColor.White;
                _repl.Script.Append("foo");
                _repl.Execute();
            }

            [Fact]
            public void SetsTheForegroundColorToCyan()
            {
                _mocks.Console.VerifySet(x=>x.ForegroundColor = ConsoleColor.Cyan, Times.Once());
            }

            [Fact]
            public void CallsExecuteOnTheScriptEngine()
            {
                _mocks.ScriptEngine.Verify(x=>x.Execute("foo", _repl.References, Repl.DefaultNamespaces, It.IsAny<ScriptPackSession>()));
            }

            [Fact]
            public void CatchesExceptionsAndWritesThemInRed()
            {
                _mocks.ScriptEngine.Setup(
                    x => x.Execute(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()))
                      .Throws<ArgumentException>();

                _repl.Script.Append("foo");
                _repl.Execute();

                _mocks.Console.VerifySet(x => x.ForegroundColor = ConsoleColor.Red);
                _mocks.Console.Verify(x => x.WriteLine(It.IsAny<string>()));

            }
            
            [Fact]
            public void SetsTheForegroundColorBackToTheDefault()
            {
                _mocks.Console.VerifySet(x=>x.ForegroundColor = ConsoleColor.White);
            }

            [Fact]
            public void ShouldProcessFileIfLineIsALoad()
            {
                var mocks = new Mocks();
                _repl = GetRepl(mocks);
                _repl.Script.Append("#load \"file.csx\"");
                _repl.Execute();
                
                mocks.FilePreProcessor.Verify(i => i.ProcessFile(It.Is<string>(x => x == "file.csx")), Times.Once());
            }

            [Fact]
            public void ShouldExecuteLoadedFileIfLineIsALoad()
            {
                var mocks = new Mocks();
                _repl = GetRepl(mocks);
                _repl.Script.Append("#load \"file.csx\"");
                _repl.Execute();

                mocks.ScriptEngine.Verify(i => i.Execute(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Once());
            }

            [Fact]
            public void ShouldReferenceAssemblyIfLineIsAReference()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Script.Append("#r \"my.dll\"");
                _repl.Execute();

                //default references = 6, + 1 we just added
                _repl.References.Count().ShouldEqual(7);
            }

            [Fact]
            public void ShouldNotExecuteAnythingIfLineIsAReference()
            {
                var mocks = new Mocks();
                mocks.FileSystem.Setup(i => i.CurrentDirectory).Returns("C:/");
                _repl = GetRepl(mocks);
                _repl.Initialize(Enumerable.Empty<string>(), Enumerable.Empty<IScriptPack>());
                _repl.Script.Append("#r \"my.dll\"");
                _repl.Execute();

                mocks.ScriptEngine.Verify(i => i.Execute(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Never());
            }
        }
    }
}
