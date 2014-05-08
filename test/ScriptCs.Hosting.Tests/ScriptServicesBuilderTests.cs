using System;
using System.Linq;
using System.Net.Configuration;
using System.Reflection;
using System.Text;
using Common.Logging;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Xunit;
using Xunit.Extensions;

namespace ScriptCs.Hosting.Tests
{
    public class ScriptServicesBuilderTests
    {
        public class TheBuildMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldResolveScriptServices(ScriptServices scriptServices, [Frozen] Mock<IRuntimeServices> runtimeServicesMock, ScriptServicesBuilder builder)
            {
                runtimeServicesMock.Setup(r => r.GetScriptServices()).Returns(scriptServices);
                builder.Overrides[typeof(IScriptEngine)] = null;
                builder.Build().ShouldEqual(scriptServices);
            }
        }

        public class TheLoadModulesMethod
        {
            [Theory, ScriptCsAutoData]
            public void ShouldLoadTheMonoModuleWhenTheMonoRuntimeIsPresent([Frozen] Mock<ITypeResolver> typeResolver, [Frozen] Mock<IModuleLoader> moduleLoader, ScriptServicesBuilder builder)
            {
                typeResolver.Setup(r => r.ResolveType("Mono.Runtime")).Returns(typeof(string));
                moduleLoader.Setup(
                    m =>
                        m.Load(It.IsAny<IModuleConfiguration>(), It.IsAny<string[]>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string[]>()))
                    .Callback<IModuleConfiguration, string[], string, string, string[]>(
                        (config, paths, hostBin, extension, module) => module.Single().ShouldEqual("mono"));
                builder.LoadModules(null);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLoadTheMonoModuleWhenTheMonoModuleIsPassedInTheListOfModules([Frozen] Mock<ITypeResolver> typeResolver, [Frozen] Mock<IModuleLoader> moduleLoader, ScriptServicesBuilder builder)
            {
                typeResolver.Setup(r => r.ResolveType("Mono.Runtime")).Returns((Type)null);
                moduleLoader.Setup(
                    m =>
                        m.Load(It.IsAny<IModuleConfiguration>(), It.IsAny<string[]>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string[]>()))
                    .Callback<IModuleConfiguration, string[], string, string, string[]>(
                        (config, paths, hostBin, extension, module) => module.Single().ShouldEqual("mono"));
                builder.LoadModules(null, "mono");
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLoadTheRoslynModuleWhenTheMonoModuleIsNotSelected([Frozen] Mock<ITypeResolver> typeResolver, [Frozen] Mock<IModuleLoader> moduleLoader, ScriptServicesBuilder builder)
            {
                typeResolver.Setup(r => r.ResolveType("Mono.Runtime")).Returns((Type)null);
                moduleLoader.Setup(
                    m =>
                        m.Load(It.IsAny<IModuleConfiguration>(), It.IsAny<string[]>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string[]>()))
                    .Callback<IModuleConfiguration, string[], string, string, string[]>(
                        (config, paths, hostBin, extension, module) => module.Single().ShouldEqual("roslyn"));
                builder.LoadModules(null);

            }

            [Theory, ScriptCsAutoData]
            public void ShouldFindAllModulesInTheFileSystem([Frozen] Mock<ITypeResolver> typeResolver, [Frozen] Mock<IModuleLoader> moduleLoader, [Frozen] Mock<IFileSystem> fileSystem, [Frozen] Mock<IInitializationServices> initializationServices, ScriptServicesBuilder builder)
            {
                typeResolver.Setup(r => r.ResolveType("Mono.Runtime")).Returns((Type)null);
                fileSystem.SetupGet(fs => fs.ModulesFolder).Returns(@"c:\modules");
                fileSystem.SetupGet(fs => fs.ModulesFolder).Returns(@"c:\current");
                fileSystem.SetupGet(fs => fs.HostBin).Returns(@"c:\hostbin");
                initializationServices.Setup(i => i.GetFileSystem()).Returns(fileSystem.Object);
                moduleLoader.Setup(
                   m =>
                       m.Load(It.IsAny<IModuleConfiguration>(), It.IsAny<string[]>(), It.IsAny<string>(),
                           It.IsAny<string>(), It.IsAny<string[]>()))
                   .Callback<IModuleConfiguration, string[], string, string, string[]>(
                       (config, paths, hostBin, extension, module) =>
                       {
                           paths.ShouldContain(@"c:\modules");
                           paths.ShouldContain(@"c:\current");
                           paths.ShouldContain(@"c:\hostbin");
                       });

            }
        }
    }
}
