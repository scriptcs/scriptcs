using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Hosting;
using Should;
using Xunit;
using AutoFixture.Xunit2;

namespace ScriptCs.Tests
{
    public class ExecuteLooseScriptCommandTests
    {
        public class ExecuteMethod
        {
            [Theory, ScriptCsAutoData]
            public void LooseScriptExecCommandShouldInvokeWithScriptPassedFromArgs(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IScriptExecutor> executor,
                [Frozen] Mock<IInitializationServices> initializationServices,
                [Frozen] Mock<IScriptServicesBuilder> servicesBuilder,
                ScriptServices services)
            {
                // arrange
                var args = new Config { AllowPreRelease = false, PackageName = "", Eval = "foo", };

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
                    i => i.ExecuteScript(It.Is<string>(x => x == "foo"), It.IsAny<string[]>()), Times.Once());

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

                var args = new Config { AllowPreRelease = false, PackageName = "", Eval = "foo", };

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
                    i => i.ExecuteScript(It.Is<string>(x => x == "foo"), It.IsAny<string[]>()), Times.Once());

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
                    Eval = "foo"
                };

                executor.Setup(i => i.ExecuteScript(It.IsAny<string>(), It.IsAny<string[]>()))
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
                    Eval = "foo"
                };

                executor.Setup(i => i.ExecuteScript(It.IsAny<string>(), It.IsAny<string[]>()))
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
                var args = new Config { Eval = "foo" };

                executor.Setup(i => i.ExecuteScript(It.IsAny<string>(), It.IsAny<string[]>()))
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
            public void ShouldComposeScripts([Frozen] Mock<IFileSystem> fileSystem, Mock<IScriptLibraryComposer> composer)
            {
                var cmd = new ExecuteLooseScriptCommand(
                    null,
                    null,
                    fileSystem.Object,
                    new Mock<IScriptExecutor>().Object,
                    new Mock<IScriptPackResolver>().Object,
                    new TestLogProvider(),
                    new Mock<IAssemblyResolver>().Object,
                    composer.Object);

                cmd.Execute();

                composer.Verify(c => c.Compose(It.IsAny<string>(), null));
            }


        }
    }
}
