using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Tests
{
    public class AppDomainAssemblyResolverTests
    {
        static AppDomainAssemblyResolverTests()
        {
            _assemblyName = typeof(Mock).Assembly.GetName();
            _info = new AssemblyInfo { Path = _assemblyName.CodeBase.Substring(8).Replace('/', Path.DirectorySeparatorChar) };
        }

        private static AssemblyName _assemblyName;
        private static AssemblyInfo _info;

        public class TheConstructor
        {
            [Theory, ScriptCsAutoData]
            public void ShouldSubscribeToTheResolveEvent(
                TestLogProvider logProvider,
                IFileSystem fileSystem,
                IAssemblyResolver assemblyResolver,
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock)
            {
                bool called = false;

                assemblyUtilityMock.Setup(a => a.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                var assemblyUtility = assemblyUtilityMock.Object;

                new AppDomainAssemblyResolver(logProvider, fileSystem, assemblyResolver,
                    assemblyUtility,
                    resolveHandler: (o, r) =>
                    {
                        called = true;
                        return Assembly.GetExecutingAssembly();
                    }
                );
                Assembly.Load("test");
                called.ShouldBeTrue();
            }
        }

        public class TheInitializeMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldAddHostAssemblyPaths(
                [Frozen] Mock<IFileSystem> fileSystemMock,
                Mock<AppDomainAssemblyResolver> resolverMock)
            {
                var hostBin = "c:\test";
                var dll = "c:\test\test.dll";

                fileSystemMock.Setup(fs => fs.EnumerateFiles(hostBin, "*.dll", SearchOption.TopDirectoryOnly)).Returns(new[] { dll });
                fileSystemMock.SetupGet(fs => fs.HostBin).Returns(hostBin);
                resolverMock.Setup(r => r.AddAssemblyPaths(It.IsAny<IEnumerable<string>>()));
                resolverMock.Object.Initialize();
                resolverMock.Verify(r => r.AddAssemblyPaths(It.Is<IEnumerable<string>>(paths => paths.Contains(dll))));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddModuleAssemblyPaths(
                [Frozen] Mock<IFileSystem> fileSystemMock,
                [Frozen] Mock<IAssemblyResolver> assemblyResolverMock,
                Mock<AppDomainAssemblyResolver> resolverMock)
            {
                var modulesFolder = "c:\test";
                var dll = "c:\test\test.dll";
                assemblyResolverMock.Setup(a => a.GetAssemblyPaths(modulesFolder, true)).Returns(new[] { dll });
                fileSystemMock.SetupGet(fs => fs.GlobalFolder).Returns(modulesFolder);
                resolverMock.Setup(r => r.AddAssemblyPaths(It.IsAny<IEnumerable<string>>()));
                resolverMock.Object.Initialize();
                resolverMock.Verify(r => r.AddAssemblyPaths(It.Is<IEnumerable<string>>(paths => paths.Contains(dll))));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldAddScriptPackAssemblyPaths(
                [Frozen] Mock<IFileSystem> fileSystemMock,
                [Frozen] Mock<IAssemblyResolver> assemblyResolverMock,
                Mock<AppDomainAssemblyResolver> resolverMock)
            {
                var scriptAssemblyPath = "c:\test";
                var dll = "c:\test\test.dll";
                assemblyResolverMock.Setup(a => a.GetAssemblyPaths(scriptAssemblyPath, true)).Returns(new[] { dll });
                fileSystemMock.SetupGet(fs => fs.CurrentDirectory).Returns(scriptAssemblyPath);
                resolverMock.Setup(r => r.AddAssemblyPaths(It.IsAny<IEnumerable<string>>()));
                resolverMock.Object.Initialize();
                resolverMock.Verify(r => r.AddAssemblyPaths(It.Is<IEnumerable<string>>(paths => paths.Contains(dll))));
            }
        }

        public class TheAddAssemblyPathsMethod
        {
            //I can't use Autofixture's [Frozen] attribute here as there appears to be a bug. If you try to freeze a Mock<IDictionary<>>
            //instead of a configurable Mock, an Dictionary<> is injected. I need to override TryGetValue for this test.
            [Fact]
            public void ShouldRetrieveTheMappedAssemblyInfo()
            {
                var assemblyUtilityMock = new Mock<IAssemblyUtility>();
                var assemblyInfoMapMock = new Mock<IDictionary<string, AssemblyInfo>>();
                assemblyUtilityMock.Setup(u => u.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                AssemblyInfo foundInfo = null;
                assemblyInfoMapMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out foundInfo)).Returns(false);

                var resolver = new AppDomainAssemblyResolver(
                    new TestLogProvider(),
                    Mock.Of<IFileSystem>(),
                    Mock.Of<IAssemblyResolver>(),
                    assemblyUtilityMock.Object,
                    assemblyInfoMapMock.Object
                );

                resolver.AddAssemblyPaths(new[] { _info.Path });
                assemblyInfoMapMock.Verify(m => m.TryGetValue(_assemblyName.Name, out foundInfo));
            }

            //Here I can use [Frozen] as I don't need to override members
            [Theory, ScriptCsAutoData]
            public void ShouldRegisterTheAssemblyIfTheAssemblyDoesNotExist(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                assemblyUtilityMock.Setup(u => u.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                resolver.AddAssemblyPaths(new[] { _info.Path });
                assemblyInfoMap.ContainsKey(_assemblyName.Name).ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldOverrideIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasNotLoaded(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                assemblyUtilityMock.Setup(u => u.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                _info.Version = new Version(0, 0);
                resolver.AddAssemblyPaths(new[] { _info.Path });
                _info = assemblyInfoMap[_assemblyName.Name];
                _info.Version.ShouldEqual(_assemblyName.Version);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLogWhenTheAssemblyIsMapped(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] TestLogProvider logProvider,
                AppDomainAssemblyResolver resolver)
            {
                assemblyUtilityMock.Setup(u => u.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                resolver.AddAssemblyPaths(new[] { _info.Path });
                logProvider.Output.ShouldContain(
                    "DEBUG: Mapping Assembly " + _assemblyName.Name + " to version:" + _assemblyName.Version);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldWarnIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasLoaded(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                [Frozen] TestLogProvider logProvider,
                AppDomainAssemblyResolver resolver)
            {
                _info.Version = new Version(0, 0);
                assemblyUtilityMock.Setup(u => u.IsManagedAssembly(It.IsAny<string>())).Returns(true);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                _info.Assembly = typeof(Mock).Assembly;
                assemblyInfoMap[_assemblyName.Name] = _info;
                resolver.AddAssemblyPaths(new[] { _info.Path });
                logProvider.Output.ShouldContain(
                    "WARN: Conflict: Assembly " + _info.Path + " with version " + _assemblyName.Version +
                    " cannot be added as it has already been resolved");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotOverrideIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasLoaded(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                _info.Version = new Version(0, 0);
                _info.Assembly = typeof(Mock).Assembly;
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                assemblyInfoMap[_assemblyName.Name] = _info;
                resolver.AddAssemblyPaths(new[] { _info.Path });
                var newInfo = assemblyInfoMap[_assemblyName.Name];
                newInfo.Version.ShouldEqual(_info.Version);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotOverrideIfTheAssemblyVersionIsLessThenOrEqualToTheMappedAssembly(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                _info.Version = new Version(99, 0, 0);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                assemblyInfoMap[_assemblyName.Name] = _info;
                resolver.AddAssemblyPaths(new[] { _info.Path });
                var newInfo = assemblyInfoMap[_assemblyName.Name];
                newInfo.Version.ShouldEqual(_info.Version);
            }
        }

        public class TheAssemblyResolveMethod
        {
            [Fact]
            public void ShouldRetrieveTheMappedAssemblyInfo()
            {
                var assemblyUtilityMock = new Mock<IAssemblyUtility>();
                var assemblyInfoMapMock = new Mock<IDictionary<string, AssemblyInfo>>();
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                AssemblyInfo foundInfo = null;
                assemblyInfoMapMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out foundInfo)).Returns(false);

                var resolver = new AppDomainAssemblyResolver(
                    new TestLogProvider(),
                    Mock.Of<IFileSystem>(),
                    Mock.Of<IAssemblyResolver>(),
                    assemblyUtilityMock.Object,
                    assemblyInfoMapMock.Object
                    );

                resolver.AssemblyResolve(this, new ResolveEventArgs(_assemblyName.Name));
                assemblyInfoMapMock.Verify(m => m.TryGetValue(_assemblyName.Name, out foundInfo));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLoadTheAssemblyIfTheMappedAssemblyInfoExistsAndItHasNotBeenLoaded(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                assemblyUtilityMock.Setup(u => u.LoadFile(_info.Path)).Returns(typeof(Mock).Assembly);
                assemblyInfoMap[_assemblyName.Name] = _info;
                var args = new ResolveEventArgs(_assemblyName.Name);
                var assembly = resolver.AssemblyResolve(this, args);
                assembly.ShouldEqual(_info.Assembly);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLogTheAssemblyThatIsBeingResolved(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] TestLogProvider logProvider,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                assemblyUtilityMock.Setup(u => u.LoadFile(_info.Path)).Returns(typeof(Mock).Assembly);
                assemblyInfoMap[_assemblyName.Name] = _info;

                resolver.AssemblyResolve(this, new ResolveEventArgs(_assemblyName.Name));
                logProvider.Output.ShouldContain(
                    "DEBUG: Resolving from: " + _assemblyName.Name + " to: " + _assemblyName.ToString());
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReturnTheAssemblyIfItWasLoaded(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(_info.Path)).Returns(_assemblyName);
                assemblyUtilityMock.Setup(u => u.LoadFile(_info.Path)).Returns(typeof(Mock).Assembly);
                assemblyInfoMap[_assemblyName.Name] = _info;
                var args = new ResolveEventArgs(_assemblyName.Name);
                var assembly = resolver.AssemblyResolve(this, args);
                assembly.ShouldEqual(_info.Assembly);
            }
        }
    }
}
