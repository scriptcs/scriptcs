using System.Collections.Generic;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptPackSessionTests
    {
        public class TheConstructor
        {
            private ScriptPackSession _scriptPackSession;
            private Mock<IScriptPack> _scriptPackMock;
            private Mock<IScriptPackContext> _contextMock;

            public TheConstructor()
            {
                _scriptPackMock = new Mock<IScriptPack>();
                _contextMock = new Mock<IScriptPackContext>();
                _scriptPackMock.Setup(p => p.GetContext()).Returns(_contextMock.Object);
                _scriptPackSession = new ScriptPackSession(new List<IScriptPack>{_scriptPackMock.Object}, new string[0]);
            }

            [Fact]
            public void ShouldPopulateContexts()
            {
                _scriptPackSession.Contexts.ShouldContain(_contextMock.Object);                
            }

            [Fact]
            public void ShouldInitializeReferences()
            {
                _scriptPackSession.References.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldInitializeNamespaces()
            {
                _scriptPackSession.Namespaces.ShouldNotBeNull();
            }

            [Fact]
            public void ShouldAddContextNamespaces()
            {
                _scriptPackSession.Namespaces.Contains(_contextMock.Object.GetType().Namespace);
            }

            [Fact]
            public void ShouldInitializeState()
            {
                _scriptPackSession.State.ShouldNotBeNull();
            }
        }

        public class TheInitializeMethod
        {
            private ScriptPackSession _scriptPackSession;
            private Mock<IScriptPack> _scriptPackMock1;
            private Mock<IScriptPack> _scriptPackMock2;
            private Mock<IScriptPackContext> _contextMock1;
            private Mock<IScriptPackContext> _contextMock2;

            public TheInitializeMethod()
            {
                _contextMock1 = new Mock<IScriptPackContext>();
                _contextMock2 = new Mock<IScriptPackContext>();
                _scriptPackMock1 = new Mock<IScriptPack>();
                _scriptPackMock1.Setup(p => p.GetContext()).Returns(_contextMock1.Object);
                _scriptPackMock2 = new Mock<IScriptPack>();
                _scriptPackMock2.Setup(p => p.GetContext()).Returns(_contextMock2.Object);
                _scriptPackSession = new ScriptPackSession(new List<IScriptPack> { _scriptPackMock1.Object, _scriptPackMock2.Object }, new string[0]);
                _scriptPackSession.InitializePacks();
            }
            
            [Fact]
            public void ShouldCallInitializeOnAllScriptPacks()
            {
                _scriptPackMock1.Verify(p => p.Initialize(_scriptPackSession));
                _scriptPackMock2.Verify(p => p.Initialize(_scriptPackSession));
            }
        }

        public class TheTerminateMethod
        {
            private ScriptPackSession _scriptPackSession;
            private Mock<IScriptPack> _scriptPackMock1;
            private Mock<IScriptPack> _scriptPackMock2;
            private Mock<IScriptPackContext> _contextMock1;
            private Mock<IScriptPackContext> _contextMock2;

            public TheTerminateMethod()
            {
                _contextMock1 = new Mock<IScriptPackContext>();
                _contextMock2 = new Mock<IScriptPackContext>();
                _scriptPackMock1 = new Mock<IScriptPack>();
                _scriptPackMock1.Setup(p => p.GetContext()).Returns(_contextMock1.Object);
                _scriptPackMock2 = new Mock<IScriptPack>();
                _scriptPackMock2.Setup(p => p.GetContext()).Returns(_contextMock2.Object);
                _scriptPackSession = new ScriptPackSession(new List<IScriptPack> { _scriptPackMock1.Object, _scriptPackMock2.Object }, new string[0]);
                _scriptPackSession.TerminatePacks();
            }

            [Fact]
            public void ShouldCallTerminateOnAllScriptPacks()
            {
                _scriptPackMock1.Verify(p => p.Terminate());
                _scriptPackMock2.Verify(p => p.Terminate());
            }
        }

        public class TheAddReferenceMethod
        {
            private ScriptPackSession _scriptPackSession = new ScriptPackSession(Enumerable.Empty<IScriptPack>(), new string[0]);

            public TheAddReferenceMethod()
            {
                ((IScriptPackSession)_scriptPackSession).AddReference("ref");
            }

            [Fact]
            public void ShouldAddTheReference()
            {
                _scriptPackSession.References.ShouldContain("ref");
            }
        }

        public class TheImportNamespaceMethod
        {
            private ScriptPackSession _scriptPackSession = new ScriptPackSession(Enumerable.Empty<IScriptPack>(), new string[0]);

            public TheImportNamespaceMethod()
            {
                ((IScriptPackSession)_scriptPackSession).ImportNamespace("ns");
            }

            public void ShouldAddTheNamespace()
            {
                _scriptPackSession.Namespaces.ShouldContain("ns");
            }
        }
    }
}
