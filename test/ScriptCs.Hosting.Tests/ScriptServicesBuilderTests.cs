using System;
using System.Reflection;
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
                builder.Overrides[typeof (IScriptEngine)] = null;
                builder.Build().ShouldEqual(scriptServices);
            }
        }

        public class TheLoadModulesMethod
        {
            [Theory]
            [ScriptCsAutoData]
            public void ShouldLoadTheMonoModuleWhenTheMonoRuntimeIsPresent(IScriptServicesBuilder builder)
            {
                /*
                var monoRuntimeTypeMock = new Mock<Type>();
                monoRuntimeTypeMock.SetupGet(t => t.Name).Returns("Runtime");
                monoRuntimeTypeMock.SetupGet(t => t.FullName).Returns("Mono.Runtime");
                monoRuntimeTypeMock.SetupGet(t => t.Namespace).Returns("Mono");
                builder.LoadModules(null);
                 * */
            }

            public void ShouldLoadTheMonoModuleWhenTheMonoModuleIsPassedInTheListOfModules()
            {
                
            }

            public void ShouldLoadTheRoslynModuleWhenTheMonoModuleIsNotSelected()
            {
                
            }

            public void ShouldFindAllModulesInTheFileSystem()
            {
                
            }
        }
    }
}
