using System;
using System.Linq;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
using ScriptCs.Logging;
using ScriptCs.Tests;
using Should;
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

            [Theory, ScriptCsAutoData]
            public void ShouldLoadScriptPacksIfReplIsTrue(IConsole console, ILog logger, IScriptEngine engine)
            {
                var builder = new ScriptServicesBuilder(console, logger);
                builder.Overrides[typeof(IScriptEngine)] = engine.GetType();
                builder.Repl();
                builder.Build();
                var runtimeServices = (RuntimeServices) builder.RuntimeServices;
                runtimeServices.InitDirectoryCatalog.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLoadScriptPacksIfScriptNameIsSet(IConsole console, ILog logger, IScriptEngine engine)
            {
                var builder = new ScriptServicesBuilder(console, logger);
                builder.Overrides[typeof(IScriptEngine)] = engine.GetType();
                builder.ScriptName("");
                builder.Build();
                var runtimeServices = (RuntimeServices)builder.RuntimeServices;
                runtimeServices.InitDirectoryCatalog.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShoulLoadScriptPacksIfLoadScriptPacksIsTrue(IConsole console, ILog logger, IScriptEngine engine)
            {
                var builder = new ScriptServicesBuilder(console, logger);
                builder.Overrides[typeof(IScriptEngine)] = engine.GetType();
                builder.LoadScriptPacks();
                builder.Build();
                var runtimeServices = (RuntimeServices)builder.RuntimeServices;
                runtimeServices.InitDirectoryCatalog.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotLoadScriptPacksIfLoadScriptPacksIsFalse(IConsole console, ILog logger, IScriptEngine engine)
            {
                var builder = new ScriptServicesBuilder(console, logger);
                builder.Overrides[typeof(IScriptEngine)] = engine.GetType();
                builder.LoadScriptPacks(false);
                builder.Build();
                var runtimeServices = (RuntimeServices)builder.RuntimeServices;
                runtimeServices.InitDirectoryCatalog.ShouldBeFalse();
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
                fileSystem.SetupGet(fs => fs.GlobalFolder).Returns(@"c:\modules");
                fileSystem.SetupGet(fs => fs.GlobalFolder).Returns(@"c:\current");
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
