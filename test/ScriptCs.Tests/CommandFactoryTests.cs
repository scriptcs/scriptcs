using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class CommandFactoryTests
    {
        public class CreateCommandMethod
        {
            private static IScriptServicesBuilder CreateBuilder(bool packagesFileExists = true, bool packagesFolderExists = true)
            {
                const string CurrentDirectory = "C:\\";
                const string PackagesFile = "C:\\packages.config";
                const string PackagesFolder = "C:\\packages";

                var fs = new Mock<IFileSystem>();
                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                fixture.Register(() => fs.Object);

                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fs.Setup(x => x.FileExists(PackagesFile)).Returns(packagesFileExists);
                fs.Setup(x => x.DirectoryExists(PackagesFolder)).Returns(packagesFolderExists);
                return fixture.Create<ScriptServicesBuilder>();
            }

            private static ScriptServices CreateRoot(bool packagesFileExists = true, bool packagesFolderExists = true)
            {
                const string CurrentDirectory = "C:\\";
                const string PackagesFile = "C:\\packages.config";
                const string PackagesFolder = "C:\\packages";

                var fs = new Mock<IFileSystem>();
                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                fixture.Register(() => fs.Object);

                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fs.Setup(x => x.FileExists(PackagesFile)).Returns(packagesFileExists);
                fs.Setup(x => x.DirectoryExists(PackagesFolder)).Returns(packagesFolderExists);
                return fixture.Create<ScriptServices>();
            }

            [Fact]
            public void ShouldInstallAndSaveWhenInstallFlagIsOn()
            {
                // Arrange
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = string.Empty,
                    ScriptName = null
                };

                // Act
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<IInstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<ISaveCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenScriptNameIsPassed()
            {
                // Arrange
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                // Act
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldInstallAndExecuteWhenScriptNameIsPassedAndPackagesFolderDoesNotExist()
            {
                // Arrange
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                // Act
                var root = CreateRoot(packagesFileExists: true, packagesFolderExists: false);
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<IInstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldExecuteWhenBothNameAndInstallArePassed()
            {
                // Arrange
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = string.Empty,
                    ScriptName = "test.csx"
                };

                // Act
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                result.ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldSaveAndCleanWhenCleanFlagIsPassed()
            {
                // Arrange
                var args = new ScriptCsArgs { Clean = true, ScriptName = null };

                // Act
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<ISaveCommand>();
                compositeCommand.Commands[1].ShouldImplement<ICleanCommand>();
            }

            [Fact]
            public void ShouldSaveWhenSaveFlagIsPassed()
            {
                // Arrange
                var args = new ScriptCsArgs { Save = true, ScriptName = null };

                // Act
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                result.ShouldNotBeNull();
                result.ShouldImplement<ISaveCommand>();
            }

            [Fact]
            public void ShouldReturnInvalidWhenNoNameOrInstallSet()
            {
                // Arrange
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = null
                };

                // Act
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                result.ShouldImplement<IInvalidCommand>();
            }

            [Fact]
            public void ShouldReturnHelpCommandWhenHelpIsPassed()
            {
                // Arrange
                var args = new ScriptCsArgs { Help = true };

                // Act
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                result.ShouldImplement<IHelpCommand>();
            }

            [Fact]
            public void ShouldPassScriptArgsToExecuteCommandConstructor()
            {
                // Arrange
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = null,
                    ScriptName = "test.csx"
                };

                // Act
                var scriptArgs = new string[0];
                var factory = new CommandFactory(CreateBuilder());
                var result = factory.CreateCommand(args, scriptArgs) as IScriptCommand;

                // Assert
                result.ScriptArgs.ShouldEqual(scriptArgs);
            }
        }
    }
}