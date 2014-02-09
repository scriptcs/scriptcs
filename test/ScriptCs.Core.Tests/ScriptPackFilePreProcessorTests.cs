using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Logging;
using NuGet;
using ScriptCs.Contracts;
using Xunit;
using Moq;
using IFileSystem = ScriptCs.Contracts.IFileSystem;
using Should;

namespace ScriptCs.Tests
{
    public class ScriptPackFilePreProcessorTests
    {
        public class ParseScriptMethod
        {
            private Mock<ScriptCs.Contracts.IFileSystem> _fileSystem = new Mock<IFileSystem>();
            private ILog _log = Mock.Of<ILog>();
            private ScriptPackFilePreProcessor _preProcessor;

            public ParseScriptMethod()
            {
                var lineProcessors = new ILineProcessor[]
                {
                    new UsingLineProcessor(),
                    new ReferenceLineProcessor(_fileSystem.Object),
                    new LoadLineProcessor(_fileSystem.Object)
                };
                _preProcessor = new ScriptPackFilePreProcessor(_fileSystem.Object, _log, lineProcessors);
            }

            [Fact]
            public void ShouldInjectTheScriptPackClassWrapperIntoLoadedScriptPackScript()
            {
                var loader = new List<string>
                {
                    @"#load Pack1ScriptPack.csx",
                    @"#load Pack2ScriptPack.csx"
                };

                var pack1 = new List<string>
                {
                    @"void Init() {",
                    @"}"
                };

                var pack2 = new List<string>
                {
                    @"void Init() {",
                    @"}"
                };

                var context = new FileParserContext();
                ConfigureFileSystemWithFile(@"c:\Pack1ScriptPack.csx", pack1);
                ConfigureFileSystemWithFile(@"c:\Pack2ScriptPack.csx", pack2);

                _preProcessor.ParseScript(loader, context);
                var body = String.Join("",  context.BodyLines);
                var regex = new Regex("(public class .+ : ScriptPackTemplate {)");
                var matches = regex.Match(body);
                matches.Groups.Count.ShouldEqual(2);
            }

            [Fact]
            public void ShouldNotInjectScriptPackClassWrapperIntoLoadedChildIfNotScriptPackScript()
            {
                var loader = new List<string>
                {
                    @"#load Script1.csx",
                };

                var script1 = new List<string>
                {
                    @"void Test() {",
                    @"}"
                };

                var context = new FileParserContext();
                ConfigureFileSystemWithFile(@"c:\Script1.csx", script1);

                _preProcessor.ParseScript(loader, context);
                var body = String.Join("", context.BodyLines);
                var regex = new Regex("(public class .+ : ScriptPackTemplate {)");
                var matches = regex.Match(body);
                matches.Groups.Count.ShouldEqual(0);
                
            }

            private void ConfigureFileSystemWithFile(string path, List<string> lines)
            {
                var file = Path.GetFileName(path);
                _fileSystem.Setup(fs => fs.GetFullPath(file)).Returns(path);
                _fileSystem.Setup(fs => fs.GetFullPath(path)).Returns(path);
                _fileSystem.Setup(fs => fs.ReadFileLines(path)).Returns(lines.ToArray);
            }

        }
    }
}
