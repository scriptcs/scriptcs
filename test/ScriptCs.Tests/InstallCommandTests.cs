using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Common.Logging;
using Moq;
using ScriptCs.Command;
using ScriptCs.Package;
using Xunit;

namespace ScriptCs.Tests
{
    public class InstallCommandTests
    {
        public class ExecuteMethod
        {
            [Fact]
            public void InstallCommandShouldInstallSinglePackageIfNamePassed()
            {
                var args = new ScriptCsArgs
                    {
                        AllowPreRelease = false,
                        Install = "mypackage",
                        ScriptName = null
                    };

                const string CurrentDirectory = @"C:\";

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);
                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);

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

                packageInstaller.Verify(i => i.InstallPackages(It.Is<IEnumerable<IPackageReference>>(x => x.Count() == 1 && x.First().PackageId == "mypackage"), It.IsAny<bool>(), It.IsAny<Action<string>>()), Times.Once());
            }

            [Fact]
            public void InstallCommandShouldInstallFromPackagesConfigIfNoNamePassed()
            {
                var args = new ScriptCsArgs
                {
                    AllowPreRelease = false,
                    Install = "",
                    ScriptName = null
                };

                const string CurrentDirectory = @"C:\";

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);
                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);

                var resolver = new Mock<IPackageAssemblyResolver>();
                var executor = new Mock<IScriptExecutor>();
                var engine = new Mock<IScriptEngine>();
                var scriptpackResolver = new Mock<IScriptPackResolver>();
                var packageInstaller = new Mock<IPackageInstaller>();
                var logger = new Mock<ILog>();
                var filePreProcessor = new Mock<IFilePreProcessor>();
                var assemblyName = new Mock<IAssemblyName>();
                var root = new ScriptServiceRoot(fs.Object, resolver.Object, executor.Object, engine.Object, filePreProcessor.Object, scriptpackResolver.Object, packageInstaller.Object, logger.Object, assemblyName.Object);

                resolver.Setup(i => i.GetPackages(It.IsAny<string>())).Returns(new List<IPackageReference>
                    {
                        new PackageReference("a", new FrameworkName(".NETFramework,Version=v4.0"), new Version()),
                        new PackageReference("b", new FrameworkName(".NETFramework,Version=v4.0"), new Version())
                    });

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                packageInstaller.Verify(i => i.InstallPackages(It.Is<IEnumerable<IPackageReference>>(x => x.Count() == 2), It.IsAny<bool>(), It.IsAny<Action<string>>()), Times.Once());
            }
        }
    }
}
