using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ploeh.AutoFixture.Xunit;
using ScriptCs.Contracts;
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
            public void ShouldLoadScriptPacksIfReplIsTrue(IConsole console, TestLogProvider logProvider, IScriptEngine engine)
            {
                var builder = new ScriptServicesBuilder(console, logProvider);
                builder.Overrides[typeof(IScriptEngine)] = engine.GetType();
                builder.Repl();
                builder.Build();
                var runtimeServices = (RuntimeServices) builder.RuntimeServices;
                runtimeServices.InitDirectoryCatalog.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldLoadScriptPacksIfScriptNameIsSet(IConsole console, TestLogProvider logProvider, IScriptEngine engine)
            {
                var builder = new ScriptServicesBuilder(console, logProvider);
                builder.Overrides[typeof(IScriptEngine)] = engine.GetType();
                builder.ScriptName("");
                builder.Build();
                var runtimeServices = (RuntimeServices)builder.RuntimeServices;
                runtimeServices.InitDirectoryCatalog.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShoulLoadScriptPacksIfLoadScriptPacksIsTrue(IConsole console, TestLogProvider logProvider, IScriptEngine engine)
            {
                var builder = new ScriptServicesBuilder(console, logProvider);
                builder.Overrides[typeof(IScriptEngine)] = engine.GetType();
                builder.LoadScriptPacks();
                builder.Build();
                var runtimeServices = (RuntimeServices)builder.RuntimeServices;
                runtimeServices.InitDirectoryCatalog.ShouldBeTrue();
            }

            [Theory, ScriptCsAutoData]
            public void ShouldNotLoadScriptPacksIfLoadScriptPacksIsFalse(IConsole console, TestLogProvider logProvider, IScriptEngine engine)
            {
                var builder = new ScriptServicesBuilder(console, logProvider);
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

        public class TheSetOverrideMethods
        {
            [Theory, ScriptCsAutoData]
            public void ShouldReturnTheBuilder(ScriptServicesBuilder builder)
            {
                var someValue = new SomeOverride();
                var returnedBuilder = builder.SetOverride<ISomeOverride, SomeOverride>(someValue);

                returnedBuilder.ShouldBeSameAs(builder);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldSetTheValueUsingTheKey(ScriptServicesBuilder builder)
            {
                var someValue = new SomeOverride();
                var key = typeof(ISomeOverride);
                builder.SetOverride<ISomeOverride, SomeOverride>(someValue);

                var overrides = builder.Overrides;
                overrides.ContainsKey(key).ShouldBeTrue();
                overrides[key].ShouldBeSameAs(someValue);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldReplaceTheValueWhenKeyAlreadyExists(ScriptServicesBuilder builder)
            {
                var key = typeof(ISomeOverride);
                var firstValue = new SomeOverride();
                var secondValue = new SomeOverride();

                builder.SetOverride<ISomeOverride, SomeOverride>(firstValue);
                builder.SetOverride<ISomeOverride, SomeOverride>(secondValue);

                var overrides = builder.Overrides;
                overrides[key].ShouldNotBeSameAs(firstValue);
                overrides[key].ShouldBeSameAs(secondValue);
            }

            [Theory, ScriptCsAutoData]
            public void ShouldUseTheValueTypeWhenNoInstanceIsProvided(ScriptServicesBuilder builder)
            {
                var key = typeof(ISomeOverride);
                builder.SetOverride<ISomeOverride, SomeOverride>();

                var overrides = builder.Overrides;
                overrides.ContainsKey(key).ShouldBeTrue();
                overrides[key].ShouldBeSameAs(typeof(SomeOverride));
            }

            public interface ISomeOverride { }
            public class SomeOverride : ISomeOverride { }
        }
    }
}
