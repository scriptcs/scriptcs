using System.Collections.Generic;
using System.IO;
using Common.Logging;
using Moq;
using ScriptCs.Command;
using ScriptCs.Contracts;
using ScriptCs.Package;
using Xunit;
using System;
using System.Linq;

namespace ScriptCs.Tests
{
    public class ExecuteScriptCommandTests
    {
        public class ExecuteMethod
        {
            [Fact]
            public void ScriptExecCommandShouldInvokeWithScriptPassedFromArgs()
            {
                var args = new ScriptCsArgs
                    {
                        AllowPreRelease = false,
                        Install = "",
                        ScriptName = "test.csx"
                    };

                var fs = new Mock<IFileSystem>();
                fs.SetupGet(x => x.CurrentDirectory).Returns("C:\\");

                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyName>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                executor.Verify(i => i.Initialize(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
                executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>()), Times.Once());
                executor.Verify(i => i.Terminate(), Times.Once());
            }

            [Fact]
            public void ShouldCreateMissingBinFolder()
            {
                const string WorkingDirectory = @"C:\";

                var binFolder = Path.Combine(WorkingDirectory, "bin");

                var args = new ScriptCsArgs { ScriptName = "test.csx" };

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(WorkingDirectory);
                fs.SetupGet(x => x.CurrentDirectory).Returns(WorkingDirectory);
                fs.Setup(x => x.DirectoryExists(binFolder)).Returns(false);

                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyName>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                fs.Verify(x => x.CreateDirectory(binFolder), Times.Once());
            }

            [Fact]
            public void NonManagedAssembliesAreExcluded()
            {
                const string nonManaged = "non-managed.dll";

                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = "",
                    ScriptName = "test.csx"
                };

                var fs = new Mock<IFileSystem>();
                fs.SetupGet(x => x.CurrentDirectory).Returns("C:\\");
                fs.Setup(x => x.EnumerateFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] {
                    "managed.dll",
                    nonManaged
                });

                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyName>();
                assemblyName.Setup(x => x.GetAssemblyName(It.Is<string>(y => y == nonManaged))).Throws(new BadImageFormatException());

                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);


                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                executor.Verify(i => i.Initialize(It.Is<IEnumerable<string>>(x => !x.Contains(nonManaged)), It.IsAny<IEnumerable<IScriptPack>>()), Times.Once());
                executor.Verify(i => i.Execute(It.Is<string>(x => x == "test.csx"), It.IsAny<string[]>()), Times.Once());
                executor.Verify(i => i.Terminate(), Times.Once());
            }
        }
    }
}
