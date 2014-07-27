using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using Xunit;
using Xunit.Extensions;
using Should;
//using IFileSystem = ScriptCs.Contracts.IFileSystem;

namespace ScriptCs.Tests
{
    public class AppDomainAssemblyResolverTests
    {

        public class TheConstructor {
            [Theory, ScriptCsAutoData]
            public void ShouldSubscribeToTheResolveEvent(
                ILog logger, 
                IFileSystem fileSystem, 
                IAssemblyResolver assemblyResolver, 
                IAssemblyUtility assemblyUtility)
            {
                bool called = false;

                var resolver = new AppDomainAssemblyResolver(logger, fileSystem, assemblyResolver,
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

                fileSystemMock.Setup(fs => fs.EnumerateFiles(hostBin, "*.dll", SearchOption.TopDirectoryOnly)).Returns(new[] {dll});
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
                assemblyResolverMock.Setup(a => a.GetAssemblyPaths(modulesFolder, false)).Returns(new[] { dll });
                fileSystemMock.SetupGet(fs => fs.ModulesFolder).Returns(modulesFolder);
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
                assemblyResolverMock.Setup(a => a.GetAssemblyPaths(scriptAssemblyPath, false)).Returns(new[] { dll });
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
                var assemblyName = typeof(Mock).Assembly.GetName();
                var info = new AssemblyInfo {Path=assemblyName.CodeBase.Substring(8).Replace('/', Path.DirectorySeparatorChar)};
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(info.Path)).Returns(assemblyName);
                AssemblyInfo foundInfo = null;
                assemblyInfoMapMock.Setup(m => m.TryGetValue(It.IsAny<string>(), out foundInfo)).Returns(false);

                var resolver = new AppDomainAssemblyResolver(
                    Mock.Of<ILog>(),
                    Mock.Of<IFileSystem>(),
                    Mock.Of<IAssemblyResolver>(),
                    assemblyUtilityMock.Object,
                    assemblyInfoMapMock.Object
                );

                resolver.AddAssemblyPaths(new[]{info.Path});
                assemblyInfoMapMock.Verify(m => m.TryGetValue(assemblyName.Name, out foundInfo));
            }

            //Here I can use [Frozen] as I don't need to override members
            [Theory, ScriptCsAutoData]
            public void ShouldRegisterTheAssemblyIfTheAssemblyDoesNotExist(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock, 
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap, 
                AppDomainAssemblyResolver resolver)
            {
                var assemblyName = typeof(Mock).Assembly.GetName();
                var info = new AssemblyInfo { Path = assemblyName.CodeBase.Substring(8).Replace('/', Path.DirectorySeparatorChar) };
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(info.Path)).Returns(assemblyName);
                resolver.AddAssemblyPaths(new[] { info.Path });
                assemblyInfoMap.ContainsKey(assemblyName.Name).ShouldBeTrue();
            }

            
            [Theory, ScriptCsAutoData]
            public void ShouldOverrideIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasNotLoaded(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                var assemblyName = typeof(Mock).Assembly.GetName();
                var info = new AssemblyInfo { Path = assemblyName.CodeBase.Substring(8).Replace('/', Path.DirectorySeparatorChar) };
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(info.Path)).Returns(assemblyName);
                info.Version = new Version(0,0);
                resolver.AddAssemblyPaths(new[] { info.Path });
                info = assemblyInfoMap[assemblyName.Name];
                info.Version.ShouldEqual(assemblyName.Version);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLogWhenTheAssemblyIsMapped(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] Mock<ILog> loggerMock,
                AppDomainAssemblyResolver resolver)
            {
                var assemblyName = typeof(Mock).Assembly.GetName();
                var info = new AssemblyInfo { Path = assemblyName.CodeBase.Substring(8).Replace('/', Path.DirectorySeparatorChar) };
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(info.Path)).Returns(assemblyName);
                resolver.AddAssemblyPaths(new[] { info.Path });
                loggerMock.Verify(
                    l => l.DebugFormat("Mapping Assembly {0} to version:{1}", assemblyName.Name, assemblyName.Version));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldWarnIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasLoaded(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                [Frozen] Mock<ILog> loggerMock,
                AppDomainAssemblyResolver resolver)
            {
                var assemblyName = typeof(Mock).Assembly.GetName();
                var info = new AssemblyInfo { Path = assemblyName.CodeBase.Substring(8).Replace('/', Path.DirectorySeparatorChar), Assembly = typeof(Mock).Assembly };
                info.Version = new Version(0,0);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(info.Path)).Returns(assemblyName);
                assemblyInfoMap[assemblyName.Name] = info;
                resolver.AddAssemblyPaths(new[] { info.Path });
                loggerMock.Verify(
                    l => l.WarnFormat("Conflict: Assembly {0} with version {1} cannot be added as it has already been resolved", info.Path, assemblyName.Version));
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotOverrideIfTheAssemblyVersionIsGreaterThanTheMappedAssemblyAndItWasLoaded(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                var assemblyName = typeof(Mock).Assembly.GetName();
                var info = new AssemblyInfo { Path = assemblyName.CodeBase.Substring(8).Replace('/', Path.DirectorySeparatorChar), Assembly = typeof(Mock).Assembly };
                info.Version = new Version(0,1,0);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(info.Path)).Returns(assemblyName);
                assemblyInfoMap[assemblyName.Name] = info;
                resolver.AddAssemblyPaths(new[] { info.Path });
                var newInfo = assemblyInfoMap[assemblyName.Name];
                newInfo.Version.ShouldEqual(info.Version);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotOverrideIfTheAssemblyVersionIsLessThenOrEqualToTheMappedAssembly(
                [Frozen] Mock<IAssemblyUtility> assemblyUtilityMock,
                [Frozen] IDictionary<string, AssemblyInfo> assemblyInfoMap,
                AppDomainAssemblyResolver resolver)
            {
                var assemblyName = typeof(Mock).Assembly.GetName();
                var info = new AssemblyInfo { Path = assemblyName.CodeBase.Substring(8).Replace('/', Path.DirectorySeparatorChar), Assembly = typeof(Mock).Assembly };
                info.Version = new Version(99, 0, 0);
                assemblyUtilityMock.Setup(u => u.GetAssemblyName(info.Path)).Returns(assemblyName);
                assemblyInfoMap[assemblyName.Name] = info;
                resolver.AddAssemblyPaths(new[] { info.Path });
                var newInfo = assemblyInfoMap[assemblyName.Name];
                newInfo.Version.ShouldEqual(info.Version);
            }
        }

        public class TheAssemblyResolveMethod
        {
            [Fact]
            public void ShouldRetrievedTheMappedAssemblyInfo()
            {
            }

            [Fact]
            public void ShouldLoadTheAssemblyIfTheMappedAssemblyInfoExistsAndItHasNotBeenLoaded()
            {
            }

            [Fact]
            public void ShouldLogTheAssemblyThatIsBeingResolved()
            {
            }

            [Fact]
            public void ShouldReturnTheAssemblyIfItWasLoaded()
            {
            }
        }
    }
}
