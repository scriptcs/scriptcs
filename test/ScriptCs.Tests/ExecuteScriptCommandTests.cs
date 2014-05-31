using System;
using System.Collections.Generic;
using System.IO;

using Common.Logging;

using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;

using System.Linq;
using System.Runtime.ExceptionServices;
using ScriptCs.Hosting;
using Should;

using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class ExecuteScriptCommandTests
    {
        public class ExecuteMethod
        {
            private const string CurrentDirectory = "C:\\";

            [Theory, ScriptCsAutoData]
            public void ScriptExecCommandShouldInvokeWithScriptPassedFromArgs(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] Mock<IInitializationServices> initializationServices,
                ScriptServices services)
            {
                // Arrange
                var args = new ScriptCsArgs { AllowPreRelease = false, Install = "", ScriptName = "test.csx" };
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var servicesBuilder = fixture.Freeze<Mock<IScriptServicesBuilder>>();

                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = fixture.Create<CommandFactory>();

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                executor.Verify(i => i.Initialize(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
                executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>()), Times.Once());
                executor.Verify(i => i.Terminate(), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void NonManagedAssembliesAreExcluded(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IAssemblyUtility> assemblyUtility,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] Mock<IInitializationServices> initializationServices,
                ScriptServices services)
            {
                // Arrange
                const string NonManaged = "non-managed.dll";

                var args = new ScriptCsArgs { AllowPreRelease = false, Install = "", ScriptName = "test.csx" };
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var servicesBuilder = fixture.Freeze<Mock<IScriptServicesBuilder>>();

                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fileSystem.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories))
                          .Returns(new[] { "managed.dll", NonManaged });

                assemblyUtility.Setup(x => x.IsManagedAssembly(It.Is<string>(y => y == NonManaged))).Returns(false);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);
                
                var factory = fixture.Create<CommandFactory>();

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                executor.Verify(i => i.Initialize(It.Is<IEnumerable<string>>(x => !x.Contains(NonManaged)), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
                executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>()), Times.Once());
                executor.Verify(i => i.Terminate(), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnErrorIfThereIsCompileException(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] Mock<ILog> logger,
                [Frozen] Mock<IInitializationServices> initializationServices,
                ScriptServices services)
            {
                // Arrange
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = "",
                    ScriptName = "test.csx"
                };
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var servicesBuilder = fixture.Freeze<Mock<IScriptServicesBuilder>>();

                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);

                executor.Setup(i => i.Execute(It.IsAny<string>(), It.IsAny<string[]>()))
                        .Returns(new ScriptResult(compilationException: new Exception("test")));

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = fixture.Create<CommandFactory>();

                // Act
                var result = factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                result.ShouldEqual(CommandResult.Error);
                logger.Verify(i => i.Error(It.IsAny<object>()), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnErrorIfThereIsExecutionException(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] Mock<ILog> logger, 
                [Frozen] Mock<IInitializationServices> initializationServices,
                ScriptServices services)
            {
                // Arrange
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = "",
                    ScriptName = "test.csx"
                };
                var fixture = new Fixture().Customize(new AutoMoqCustomization());
                var servicesBuilder = fixture.Freeze<Mock<IScriptServicesBuilder>>();


                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);

                executor.Setup(i => i.Execute(It.IsAny<string>(), It.IsAny<string[]>()))
                        .Returns(new ScriptResult(executionException: new Exception("test")));

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = fixture.Create<CommandFactory>();

                // Act
                var result = factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                result.ShouldEqual(CommandResult.Error);
                logger.Verify(i => i.Error(It.IsAny<object>()), Times.Once());
            }
        }
    }
}
