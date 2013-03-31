using log4net;
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
                var logger = new Mock<ILog>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object);

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

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                (compositeCommand.Commands[0] is IInstallCommand).ShouldBeTrue();
                (compositeCommand.Commands[1] is IRestoreCommand).ShouldBeTrue();
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
            public void ShouldRestoreWhenBothNameAndRestoreArePassed()
            {
                var args = new ScriptCsArgs { Restore = true, ScriptName = "" };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args);

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                (compositeCommand.Commands[0] is IRestoreCommand).ShouldBeTrue();
                (compositeCommand.Commands[1] is IScriptCommand).ShouldBeTrue();
            }

            [Fact]
            public void ShouldCleanWhenCleanFlagIsPassed()
            {
                var args = new ScriptCsArgs { Clean = true, ScriptName = null };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args);

                result.ShouldNotBeNull();
                result.ShouldImplement<ICleanCommand>();
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
