using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Common.Logging;
using Moq;

using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

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

                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);
                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fixture.Register(() => fs.Object);

                var packageInstaller = new Mock<IPackageInstaller>();
                fixture.Register(() => packageInstaller.Object);

                var root = fixture.Create<ScriptServiceRoot>();

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

                var fixture = new Fixture().Customize(new AutoMoqCustomization());

                var fs = new Mock<IFileSystem>();
                fs.Setup(x => x.GetWorkingDirectory(It.IsAny<string>())).Returns(CurrentDirectory);
                fs.SetupGet(x => x.CurrentDirectory).Returns(CurrentDirectory);
                fixture.Register(() => fs.Object);

                var resolver = new Mock<IPackageAssemblyResolver>();
                resolver.Setup(i => i.GetPackages(It.IsAny<string>())).Returns(new List<IPackageReference>
                    {
                        new PackageReference("a", new FrameworkName(".NETFramework,Version=v4.0"), new Version()),
                        new PackageReference("b", new FrameworkName(".NETFramework,Version=v4.0"), new Version())
                    });
                fixture.Register(() => resolver.Object);

                var packageInstaller = new Mock<IPackageInstaller>();
                fixture.Register(() => packageInstaller.Object);

                var root = fixture.Create<ScriptServiceRoot>();

                var factory = new CommandFactory(root);
                var result = factory.CreateCommand(args, new string[0]);

                result.Execute();

                packageInstaller.Verify(i => i.InstallPackages(It.Is<IEnumerable<IPackageReference>>(x => x.Count() == 2), It.IsAny<bool>(), It.IsAny<Action<string>>()), Times.Once());
            }
        }
    }
}
