using System.Collections;
using System.Collections.Generic;
using System.Text;
using Common.Logging;
using Moq;

using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Should;

using Xunit.Extensions;
using System;

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
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                [Frozen] Mock<IInitializationServices> initializationServices,
                ScriptServices services)
            {
                // Arrange
                var readLines = 0;
                var builder = new StringBuilder();
                var args = new ScriptCsArgs { Repl = true };

                fileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");
                servicesBuilder.Setup(b => b.Build()).Returns(services);
                servicesBuilder.SetupGet(b => b.ConsoleInstance).Returns(console.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);

                console.Setup(x => x.ReadLine()).Callback(() => readLines++).Throws(new Exception());
                console.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(value => builder.Append(value));

                var factory = new CommandFactory(servicesBuilder.Object);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                builder.ToString().EndsWith("> ").ShouldBeTrue();
                readLines.ShouldEqual(1);
            }

            [Theory, ScriptCsAutoData]
            public void WhenPassedAScript_ShouldPressedReplWithScript(
                [Frozen] Mock<IScriptEngine> scriptEngine, 
                [Frozen] Mock<IFileSystem> fileSystem, 
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                [Frozen] Mock<IInitializationServices> initializationServices,
                ScriptServices services)
            {
                // Arrange
                var args = new ScriptCsArgs { Repl = true, ScriptName = "test.csx" };

                console.Setup(x => x.ReadLine()).Returns(() =>
                {
                    console.Setup(x => x.ReadLine()).Throws(new Exception());
                    return string.Empty;
                });
                fileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");
                servicesBuilder.Setup(b => b.Build()).Returns(services);
                //initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                scriptEngine.Setup(
                    x => x.Execute("#load test.csx", It.IsAny<string[]>(), It.IsAny<AssemblyReferences>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()));

                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);

                var factory = new CommandFactory(servicesBuilder.Object);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                scriptEngine.Verify();
            }

            [Theory, ScriptCsAutoData]
            public void WhenNotPassedAScript_ShouldNotCallTheEngineAutomatically(
                [Frozen] Mock<IScriptEngine> scriptEngine, 
                [Frozen] Mock<IFileSystem> fileSystem, 
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                [Frozen] Mock<IInitializationServices> initializationServices,
                ScriptServices services)
            {
                // Arrange
                var args = new ScriptCsArgs { Repl = true };

                console.Setup(x => x.ReadLine()).Returns(() =>
                {
                    console.Setup(x => x.ReadLine()).Throws(new Exception());
                    return string.Empty;
                });
                fileSystem.SetupGet(x => x.CurrentDirectory).Returns("C:\\");
                servicesBuilder.Setup(b => b.Build()).Returns(services);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);

                var factory = new CommandFactory(servicesBuilder.Object);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                scriptEngine.Verify(
                    x => x.Execute(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<AssemblyReferences>(), It.IsAny<IEnumerable<string>>(), It.IsAny<ScriptPackSession>()), Times.Never());
            }

            [Theory, ScriptCsAutoData]
            public void MigratesTheFileSystem(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IConsole> console,
                [Frozen] Mock<IFileSystemMigrator> fileSystemMigrator)
            {
                // Arrange
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
                    fileSystemMigrator.Object);

                // Act
                sut.Execute();

                // Assert
                fileSystemMigrator.Verify(m => m.Migrate(), Times.Once);
            }
        }
    }
}
