using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Xunit;

namespace ScriptCs.Hosting.Tests
{
    public class ScriptPackAuthoringHostTests
    {
        public class TheScriptPackMethod
        {
            private ScriptPackAuthoringHost _host;
            private IScriptPackSettings _settings;
            private IScriptPackSettingsReferences _scriptPackSettingsReferences;

            public TheScriptPackMethod()
            {
                _host = new ScriptPackAuthoringHost(null, null);
                _scriptPackSettingsReferences = _host.ScriptPack<ScriptPackTemplateTests.FakeScriptPackContext>();
                _settings = (IScriptPackSettings)_scriptPackSettingsReferences;
            }

            [Fact]
            public void ShouldReturnAScriptPackSettingsInstance()
            {
                _scriptPackSettingsReferences.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldSetTheScriptPackContext()
            {
                _settings.GetContextType().ShouldEqual(typeof(ScriptPackTemplateTests.FakeScriptPackContext));
            }
        }
    }
}
