﻿using Common.Logging;
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
            private static ScriptServiceRoot CreateRoot(bool packagesFileExists = true)
            {
                const string CurrentDirectory = "C:\\";
                const string PackagesFile = "C:\\packages.config";

                var fs = new Mock<IFileSystem>();
                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fs.Setup(x => x.FileExists(PackagesFile)).Returns(packagesFileExists);

                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object);

                return root;
            }

            [Fact]
            public void ShouldInstallAndRestoreWhenInstallFlagIsOn()
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

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<IInstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<IRestoreCommand>();
            }

            [Fact]
            public void ShouldInstallRestoreAndSaveWhenInstallFlagIsOnAndNoPackagesFileExists()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreReleaseFlag = false,
                    Install = "",
                    ScriptName = null
                };

                var factory = new CommandFactory(CreateRoot(packagesFileExists: false));
                var result = factory.CreateCommand(args);

                var compositeCommand = result as ICompositeCommand;
                compositeCommand.ShouldNotBeNull();

                compositeCommand.Commands.Count.ShouldEqual(3);
                compositeCommand.Commands[0].ShouldImplement<IInstallCommand>();
                compositeCommand.Commands[1].ShouldImplement<IRestoreCommand>();
                compositeCommand.Commands[2].ShouldImplement<ISaveCommand>();
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

                compositeCommand.Commands.Count.ShouldEqual(2);
                compositeCommand.Commands[0].ShouldImplement<IRestoreCommand>();
                compositeCommand.Commands[1].ShouldImplement<IScriptCommand>();
            }

            [Fact]
            public void ShouldSaveAndCleanWhenCleanFlagIsPassed()
            {
                var args = new ScriptCsArgs { Clean = true, ScriptName = null };

                var factory = new CommandFactory(CreateRoot());
                var result = factory.CreateCommand(args);

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
                var result = factory.CreateCommand(args);

                result.ShouldNotBeNull();
                result.ShouldImplement<ISaveCommand>();
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
