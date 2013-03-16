using System.Collections.Generic;
using Moq;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Package;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptExecuteCommandTests
    {
        public class ExecuteMethod
        {
            [Fact]
            public void ScriptExecCommandShouldInvokeWithScriptPassedFromArgs()
            {
                var args = new ScriptCsArgs
                    {
                        AllowPreReleaseFlag = false,
                        Install = "",
                        ScriptName = "test.csx"
                    };

                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, scriptpackResolver.Object, packageInstaller.Object);

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args);

                result.Execute();

                executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
            }
        }
    }
}