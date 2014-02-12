using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;
using ScriptCs.Tests;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ExtendedScriptHostTests
    {
        public class TheScriptPackMethod
        {
            private IExtendedScriptHost _host1;
            private IScriptPackSettings _settings;
            private IScriptPackSettingsReferences _scriptPackSettingsReferences1;
            private IScriptPackSettingsReferences _scriptPackSettingsReferences2;
            private IExtendedScriptHost _host2;          

            public TheScriptPackMethod()
            {
                _host1 = new ExtendedScriptHost(null, null);
                _host2 = new ExtendedScriptHost(null, null);
                _scriptPackSettingsReferences1 = _host1.ScriptPack<FakeScriptPackContext>();
                _settings = (IScriptPackSettings)_scriptPackSettingsReferences1;
                _scriptPackSettingsReferences2 = _host2.ScriptPack<FakeScriptPack>();
            }

            [Fact]
            public void ShouldReturnAScriptPackSettingsInstanceWhenPassedAScriptPackContextType()
            {
                _scriptPackSettingsReferences1.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldStoreTheScriptPackSettingsInstanceWhenPassedAScriptPackContextType()
            {
               _host1.ScriptPackSettings.ShouldEqual(_scriptPackSettingsReferences1);
            }

            [Fact]
            public void ShouldReturnNullWhenPassedAScriptPack()
            {
                _scriptPackSettingsReferences2.ShouldBeNull();
            }

            [Fact]
            public void WhenPassedAScriptPackTypeShouldStoreIt()
            {
                _host2.ScriptPackType.ShouldEqual(typeof(FakeScriptPack));
            }

            [Fact]
            public void ShouldFailIfPassedATypeWhichIsNotAScriptPackContext()
            {
                var host = new ExtendedScriptHost(null, null);
                Assert.Throws<ArgumentException>(()=>{
                    host.ScriptPack<string>();
                });
            }

            [Fact]
            public void ShouldSetTheScriptPackContext()
            {
                _settings.GetContextType().ShouldEqual(typeof(FakeScriptPackContext));
            }
        }

        public class FakeScriptPackContext : IScriptPackContext
        {
        }

        public class FakeScriptPack : IScriptPack
        {
            public void Initialize(IScriptPackSession session)
            {
                throw new NotImplementedException();
            }

            public IScriptPackContext GetContext()
            {
                throw new NotImplementedException();
            }

            public void Terminate()
            {
                throw new NotImplementedException();
            }
        }

    }
}
