using System.Diagnostics;
using Common.Logging;
using Moq;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Package;
using System.IO;
using Xunit;

namespace ScriptCs.Tests
{
    public class VersionCommandTests
    {
        public class ExecuteMethod
        {
            [Fact]
            public void VersionCommandShouldOutputVersion()
            {
                var assembly = typeof(ScriptCsArgs).Assembly;
                var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

                string actual = null;

                var args = new ScriptCsArgs { Version = true };

                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyResolver>();
                var mockConsole = new Mock<IConsole>();
                mockConsole.Setup(x => x.WriteLine(It.IsAny<string>())).Callback<string>(text => actual = text);
                var root = new ScriptServices(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object, mockConsole.Object);

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                Assert.Contains(version, actual);
            }
        }
    }
}
