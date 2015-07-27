using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Should;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class ExecuteScriptCommandTests
    {
        public class ExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void ScriptExecCommandShouldInvokeWithScriptPassedFromArgs(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                var args = new Config { AllowPreRelease = false, PackageName = "", ScriptName = "test.csx", };

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                sut.Execute();

                // assert
                executor.Verify(
                    i => i.Initialize(
                        It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());

                executor.Verify(
                    i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>()), Times.Once());

                executor.Verify(
                    i => i.Terminate(), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void NonManagedAssembliesAreExcluded(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IAssemblyUtility> assemblyUtility,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                const string NonManaged = "non-managed.dll";

                var args = new Config { AllowPreRelease = false, PackageName = "", ScriptName = "test.csx", };

                fileSystem.Setup(
                        x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories))
                    .Returns(new[] { "managed.dll", NonManaged });

                assemblyUtility.Setup(x => x.IsManagedAssembly(It.Is<string>(y => y == NonManaged))).Returns(false);
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                sut.Execute();

                // assert
                executor.Verify(
                    i => i.Initialize(
                        It.Is<IEnumerable<string>>(x => !x.Contains(NonManaged)), It.IsAny<IEnumerable<IScriptPack>>()),
                    Times.Once());

                executor.Verify(
                    i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>()), Times.Once());

                executor.Verify(
                    i => i.Terminate(), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnErrorIfThereIsCompileException(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] TestLogProvider logProvider,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                var args = new Config
                {
                    AllowPreRelease = false,
                    PackageName = "",
                    ScriptName = "test.csx",
                };

                executor.Setup(i => i.Execute(It.IsAny<string>(), It.IsAny<string[]>()))
                    .Returns(new ScriptResult(compilationException: new Exception("test")));

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                var result = sut.Execute();

                // assert
                result.ShouldEqual(CommandResult.Error);
                logProvider.Output.ShouldContain("ERROR:");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnErrorIfThereIsExecutionException(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] TestLogProvider logProvider,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                var args = new Config
                {
                    AllowPreRelease = false,
                    PackageName = "",
                    ScriptName = "test.csx"
                };

                executor.Setup(i => i.Execute(It.IsAny<string>(), It.IsAny<string[]>()))
                    .Returns(new ScriptResult(executionException: new Exception("test")));

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                var result = sut.Execute();

                // assert
                result.ShouldEqual(CommandResult.Error);
                logProvider.Output.ShouldContain("ERROR:");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnErrorIfTheScriptIsIncomplete(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] TestLogProvider logProvider,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                var args = new Config { ScriptName = "test.csx" };

                executor.Setup(i => i.Execute(It.IsAny<string>(), It.IsAny<string[]>()))
                    .Returns(ScriptResult.Incomplete);

                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                servicesBuilder.SetupGet(b => b.InitializationServices).Returns(initializationServices.Object);
                servicesBuilder.Setup(b => b.Build()).Returns(services);

                var factory = new CommandFactory(servicesBuilder.Object);
                var sut = factory.CreateCommand(args, new string[0]);

                // act
                var result = sut.Execute();

                // assert
                result.ShouldEqual(CommandResult.Error);
                logProvider.Output.ShouldContain("ERROR:");
            }

            [Theory, ScriptCsAutoData]
            public void MigratesTheFileSystem(
                [Frozen] Mock<IFileSystem> fileSystem, [Frozen] Mock<IFileSystemMigrator> fileSystemMigrator)
            {
                // arrange
                var sut = new ExecuteScriptCommand(
                    null,
                    null,
                    fileSystem.Object,
                    new Mock<IScriptExecutor>().Object,
                    new Mock<IScriptPackResolver>().Object,
                    new TestLogProvider(),
                    new Mock<IAssemblyResolver>().Object,
                    fileSystemMigrator.Object,
                    new Mock<IScriptLibraryComposer>().Object);

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
                    new TestLogProvider(),
                    new Mock<IAssemblyResolver>().Object,
                    new Mock<IFileSystemMigrator>().Object,
                    composer.Object);

                cmd.Execute();

                composer.Verify(c=>c.Compose(It.IsAny<string>(),null));
            }

            
        }
    }
}
