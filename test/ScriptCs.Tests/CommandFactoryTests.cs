using Moq;

using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

using ScriptCs.Command;

using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class CommandFactoryTests
    {
        public class CreateCommandMethod
        {
            private static ScriptServiceRoot CreateRoot(bool packagesFileExists = true, bool packagesFolderExists = true)
            {
                const string CurrentDirectory = "C:\\";
                const string PackagesFile = "C:\\packages.config";
                const string PackagesFolder = "C:\\packages";

                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var fs = new Mock<IFileSystem>();
                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fs.Setup(x => x.FileExists(PackagesFile)).Returns(packagesFileExists);
                fs.Setup(x => x.DirectoryExists(PackagesFolder)).Returns(packagesFolderExists);

                fixture.Register(() => fs.Object);

                return fixture.Create<ScriptServiceRoot>();
            }

            [Fact]
            public void ShouldInstallAndRestoreWhenInstallFlagIsOn()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = "",
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args, new string[0]);

                result.ShouldImplement<IInstallCommand>();
            }

            [Fact]
            public void ShouldInstallAndSaveWhenInstallFlagIsOnAndNoPackagesFileExists()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = "",
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot(packagesFileExists: false));
                var result = factory.CreateCommand(args, new string[0]);

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<IInstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<ISaveCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenScriptNameIsPassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args, new string[0]);

                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldInstallAndExecuteWhenScriptNameIsPassedAndPackagesFolderDoesNotExist()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                var root = CreateRoot(packagesFileExists: true, packagesFolderExists: false);
                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<IInstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenBothNameAndInstallArePassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = "",
                    ScriptName = "test.csx"
                };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args, new string[0]);

                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldSaveAndCleanWhenCleanFlagIsPassed()
            {
                var args = new ScriptCsArgs { Clean = true, ScriptName = null };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args, new string[0]);

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<ISaveCommand>();
                compositeCommand.Commands[1].ShouldImplement<ICleanCommand>();
            }

            [Fact]
            public void ShouldSaveWhenSaveFlagIsPassed()
            {
                var args = new ScriptCsArgs { Save = true, ScriptName = null };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args, new string[0]);

                result.ShouldNotBeNull();
                result.ShouldImplement<ISaveCommand>();
            }

            [Fact]
            public void ShouldReturnInvalidWhenNoNameOrInstallSet()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args, new string[0]);

                result.ShouldImplement<IInvalidCommand>();
            }

            [Fact]
            public void ShouldReturnHelpCommandWhenHelpIsPassed()
            {
                var args = new ScriptCsArgs
                    {
                        Help = true
                    };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args, new string[0]);

                result.ShouldImplement<IHelpCommand>();
            }

            [Fact]
            public void ShouldPassScriptArgsToExecuteCommandConstructor()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                var scriptArgs = new string[0];
                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args, scriptArgs) as IScriptCommand;

                result.ScriptArgs.ShouldEqual(scriptArgs);
            }
        }
    }
}
