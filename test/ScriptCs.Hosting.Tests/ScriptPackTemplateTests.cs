using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;
using Xunit;
using Should;
using Moq;

namespace ScriptCs.Tests
{
    public class ScriptPackTemplateTests
    {
        public class TheConstructor
        {
            [Fact]
            public void ShouldInvokeTheInitMethodOnTheScriptPack()
            {
                var scriptPackMock = new Mock<FakeScriptPack1>();
                var scriptPack = scriptPackMock.Object;
                scriptPackMock.Verify(s=>s.Init(), Times.Once());
            }

            [Fact]
            public void ShouldFailIfInitMethodIsNotPresent()
            {
                var scriptPackMock = new Mock<FakeScriptPack2>();
                Assert.Throws<TargetInvocationException>(() =>
                {
                    var scriptPack = scriptPackMock.Object;
                });
            }
        }

        public class TheInitializeMethod
        {
            private Mock<IScriptPackSettings> _settingsMock = new Mock<IScriptPackSettings>();
            private ScriptPackSession _session;

            public TheInitializeMethod()
            {
                _settingsMock.Setup(s => s.GetImports()).Returns(new List<string> {"Namespace1"});
                _settingsMock.Setup(s => s.GetReferences()).Returns(new List<string> {"Reference1" });
                var pack = (IScriptPack) new FakeScriptPack1(_settingsMock.Object);
                _session = new ScriptPackSession(Enumerable.Empty<IScriptPack>(), null);
                pack.Initialize(_session);

            }

            [Fact]
            public void ShouldAddNamespacesToTheSession()
            {
                _session.Namespaces.ShouldContain("Namespace1");
            }

            [Fact]
            public void ShouldAddReferencesToTheSession()
            {
                _session.References.ShouldContain("Reference1");
            }
        }

        public class TheScriptPackMethod
        {
            private FakeScriptPack1 _template;
            private IScriptPackSettings _settings;
            private IScriptPackSettingsReferences _scriptPackSettingsReferences;

            public TheScriptPackMethod()
            {
                _template = new FakeScriptPack1();
                _scriptPackSettingsReferences = _template.ScriptPack<FakeScriptPackContext>();
                _settings = (IScriptPackSettings) _scriptPackSettingsReferences;
            }

            [Fact]
            public void ShouldReturnAScriptPackSettingsInstance()
            {
                _scriptPackSettingsReferences.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldSetTheScriptPackContext()
            {
                _settings.GetContextType().ShouldEqual(typeof(FakeScriptPackContext));
            }
        }

        public class TheGetContextMethod
        {
            private FakeScriptPack1 _template;
            private IScriptPackContext _context;
            private bool _called = false;
            
            public TheGetContextMethod()
            {
                _template = new FakeScriptPack1();
                _template.ContextResolver = t =>
                {
                    _called = true;
                    return new FakeScriptPackContext();
                };
                _context = _template.ContextResolver(typeof (FakeScriptPackContext));
            }

            public void ShouldResolveTheContext()
            {
                _called.ShouldBeTrue();
                _context.ShouldNotBeNull();
            }
        }

        public class FakeScriptPackContext : IScriptPackContext
        {
            
        }

        public class FakeScriptPack1 : ScriptPackTemplate
        {
            public FakeScriptPack1()
            {
            }

            public FakeScriptPack1(IScriptPackSettings settings):base(settings)
            {
            }

            //Init is only virtual here to allow mocking with Moq.
            public virtual void Init()
            {
            }
        }

        public class FakeScriptPack2 : ScriptPackTemplate
        {
            
        }
    }
}
