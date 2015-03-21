﻿#pragma warning disable 618
using System;
using System.Collections.Generic;
using System.Text;
using Common.Logging;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class ExecuteReplCommandTests
    {
        public class TheExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldPromptForInput(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                var args = new ScriptCsArgs { Repl = true };
                var readLines = 0;
                var builder = new StringBuilder();

                console.Setup(x => x.ReadLine()).Callback(() => readLines++).Throws(new Exception());
                console.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(value => builder.Append(value));
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.SetupGet(b => b.ConsoleInstance).Returns(console.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                sut.Execute();

                // assert
                builder.ToString().EndsWith("> ").ShouldBeTrue();
                readLines.ShouldEqual(1);
            }

            [Theory, ScriptCsAutoData]
            public void WhenPassedAScript_ShouldPressedReplWithScript(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                var args = new ScriptCsArgs { Repl = true, ScriptName = "test.csx", };

                scriptEngine.Setup(x => x.Execute(
                    "#load test.csx",
                    It.IsAny<string[]>(),
                    It.IsAny<AssemblyReferences>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<ScriptPackSession>()));

                console.Setup(x => x.ReadLine()).Throws(new Exception());
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                sut.Execute();

                // assert
                scriptEngine.Verify();
            }

            [Theory, ScriptCsAutoData]
            public void WhenNotPassedAScript_ShouldNotCallTheEngineAutomatically(
                [Frozen] Mock<IScriptEngine> scriptEngine,
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                var args = new ScriptCsArgs { Repl = true };

                console.Setup(x => x.ReadLine()).Throws(new Exception());
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                sut.Execute();

                // assert
                scriptEngine.Verify(
                    x => x.Execute(
                        It.IsAny<string>(),
                        It.IsAny<string[]>(),
                        It.IsAny<AssemblyReferences>(),
                        It.IsAny<IEnumerable<string>>(),
                        It.IsAny<ScriptPackSession>()),
                    Times.Never());
            }

            [Theory, ScriptCsAutoData]
            public void MigratesTheFileSystem(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IFileSystemMigrator> fileSystemMigrator,
                IScriptLibraryComposer composer)
            {
                // arrange
                console.Setup(c => c.ReadLine()).Throws(new Exception());
                var sut = new ExecuteReplCommand(
                    null,
                    null,
                    fileSystem.Object,
                    new Mock<IScriptPackResolver>().Object,
                    new Mock<IRepl>().Object,
                    new Mock<ILog>().Object,
                    console.Object,
                    new Mock<IAssemblyResolver>().Object,
                    fileSystemMigrator.Object,
                    composer
                    );

                // act
                sut.Execute();

                // assert
                fileSystemMigrator.Verify(m => m.Migrate(), Times.Once);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldComposeScripts([Frozen] Mock<IFileSystem> fileSystem, Mock<IScriptLibraryComposer> composer)
            {
                var cmd = new ExecuteScriptCommand(
                    null,
                    null,
                    fileSystem.Object,
                    new Mock<IScriptExecutor>().Object,
                    new Mock<IScriptPackResolver>().Object,
                    new Mock<ILog>().Object,
                    new Mock<IAssemblyResolver>().Object,
                    new Mock<IFileSystemMigrator>().Object,
                    composer.Object);

                cmd.Execute();

                composer.Verify(c => c.Compose(It.IsAny<string>(),null));
            }
        }
    }
}
