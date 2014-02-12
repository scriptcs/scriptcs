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

        public class TheGetContextMethod
        {
            private FakeScriptPack1 _template;
            private IScriptPackContext _context;
            private bool _called = false;
            
            public TheGetContextMethod()
            {
                _template = new FakeScriptPack1(null);
                _template.ContextResolver = t =>
                {
                    _called = true;
                    return new FakeScriptPackContext();
                };
                _context = _template.ContextResolver(typeof (FakeScriptPackContext));
            }

            [Fact]
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
            public FakeScriptPack1(IScriptPackSettings settings):base(settings)
            {
            }
        }

        public class FakeScriptPack2 : ScriptPackTemplate
        {
            public FakeScriptPack2(IScriptPackSettings settings):base(settings)
            {
            }
            
        }
    }
}
