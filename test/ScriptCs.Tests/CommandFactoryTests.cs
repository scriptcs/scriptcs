﻿using Moq;
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
                
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var fileSystem = fixture.Freeze<Mock<IFileSystem>>();
                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fileSystem.Setup(x => x.FileExists(PackagesFile)).Returns(packagesFileExists);
                fileSystem.Setup(x => x.DirectoryExists(PackagesFolder)).Returns(packagesFolderExists);

                var builder = fixture.Freeze<Mock<IScriptServicesBuilder>>();
                var services = fixture.Create<ScriptServices>();
                builder.Setup(b => b.Build()).Returns(services);

                var initServices = fixture.Freeze<Mock<IInitializationServices>>();
                initServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                builder.SetupGet(b => b.InitializationServices).Returns(initServices.Object);
                return fixture.Create<IScriptServicesBuilder>();
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
                var compositeCommand = result as CompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<InstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<SaveCommand>();
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
                result.ShouldImplement<ExecuteScriptCommand>();
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
                var factory = new CommandFactory(CreateBuilder(true, false));
                var result = factory.CreateCommand(args, new string[0]);

                // Assert
                var compositeCommand = result as CompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<InstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<ExecuteScriptCommand>();
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
                result.ShouldImplement<ExecuteScriptCommand>();
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
                var compositeCommand = result as CompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<SaveCommand>();
                compositeCommand.Commands[1].ShouldImplement<CleanCommand>();
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
                result.ShouldImplement<SaveCommand>();
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
                result.ShouldImplement<ShowUsageCommand>();
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
                result.ShouldImplement<ShowUsageCommand>();
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
                var result = factory.CreateCommand(args, scriptArgs) as ExecuteScriptCommand;

                // Assert
                result.ScriptArgs.ShouldEqual(scriptArgs);
            }
        }
    }
}