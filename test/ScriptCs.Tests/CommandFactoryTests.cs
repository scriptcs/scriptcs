using Moq;
using ScriptCs.Command;
using ScriptCs.Package;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class CommandFactoryTests
    {
        public class CreateCommandMethod
        {
            private static ScriptServiceRoot CreateRoot()
            {
                var fs = new Mock<IFileSystem>();
                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, scriptpackResolver.Object, packageInstaller.Object);

                return root;
            }

            [Fact]
            public void ShouldInstallWhenInstallFlagIsOn()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = "",
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IInstallCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenScriptNameIsPassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenBothNameAndInstallArePassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = "",
                    ScriptName = "test.csx"
                };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldRestoreWhenRestoreIsPassed()
            {
                var args = new ScriptCsArgs { Restore = "" };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IRestoreCommand>();
            }

            [Fact]
            public void ShouldReturnInvalidWhenNoNameOrInstallSet()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = null,
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args);

                result.ShouldImplement<IInvalidCommand>();
            }
        }
    }
}
