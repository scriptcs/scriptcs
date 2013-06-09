using System;
using System.Collections.Generic;
using System.IO;

using Common.Logging;

using Moq;

using Ploeh.AutoFixture.Xunit;

using ScriptCs.Command;
using ScriptCs.Contracts;

using System.Linq;

using ScriptCs.Package;

using Xunit;
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
                CommandFactory factory)
            {
                // Arrange
                var args = new ScriptCsArgs { AllowPreRelease = false, Install = "", ScriptName = "test.csx" };

                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                executor.Verify(i => i.Initialize(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
                executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>()), Times.Once());
                executor.Verify(i => i.Terminate(), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldCreateMissingBinFolder([Frozen] Mock<IFileSystem> fileSystem, CommandFactory factory)
            {
                // Arrange
                var binFolder = Path.Combine(CurrentDirectory, Constants.BinFolder);

                var args = new ScriptCsArgs { ScriptName = "test.csx" };

                fileSystem.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);
                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fileSystem.Setup(x => x.DirectoryExists(binFolder)).Returns(false);

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                fileSystem.Verify(x => x.CreateDirectory(binFolder), Times.Once());
            }

            [Theory, ScriptCsAutoData]
            public void NonManagedAssembliesAreExcluded(
                [Frozen] Mock<IFileSystem> fileSystem,
                [Frozen] Mock<IAssemblyName> assemblyName,
                [Frozen] Mock<IScriptExecutor> executor,
                CommandFactory factory)
            {
                // Arrange
                const string NonManaged = "non-managed.dll";

                var args = new ScriptCsArgs { AllowPreRelease = false, Install = "", ScriptName = "test.csx" };

                fileSystem.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fileSystem.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>()))
                          .Returns(new[] { "managed.dll", NonManaged });

                assemblyName.Setup(x => x.GetAssemblyName(It.Is<string>(y => y == NonManaged))).Throws(new BadImageFormatException());

                // Act
                factory.CreateCommand(args, new string[0]).Execute();

                // Assert
                executor.Verify(i => i.Initialize(It.Is<IEnumerable<string>>(x => !x.Contains(NonManaged)), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
                executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>()), Times.Once());
                executor.Verify(i => i.Terminate(), Times.Once());
            }
        }
    }
}
