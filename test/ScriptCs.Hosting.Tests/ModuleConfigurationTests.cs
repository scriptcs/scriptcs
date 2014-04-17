using System;
using System.Collections.Generic;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class ModuleConfigurationTests
    {
        public class TheLineProcessorMethod
        {
            [Fact]
            public void ShouldAddTheLineProcessorTypeToTheOverridesDictionary()
            {
                var overrides = new Dictionary<Type, object>();

                var moduleConfiguration = new ModuleConfiguration(false, "script1.csx", false, LogLevel.Debug, overrides);
                moduleConfiguration.LineProcessor<UsingLineProcessor>();

                var processors = overrides[typeof(ILineProcessor)] as List<Type>;
                processors.ShouldContain(typeof(UsingLineProcessor));
            }

            [Fact]
            public void ShouldReturnTheModuleConfiguration()
            {
                var moduleConfiguration = new ModuleConfiguration(false, "script1.csx", false, LogLevel.Debug, new Dictionary<Type, object>());
                var config = moduleConfiguration.LineProcessor<UsingLineProcessor>();
                config.ShouldImplement<IModuleConfiguration>();
                config.ShouldEqual(moduleConfiguration);
            }
        }
    }
}
