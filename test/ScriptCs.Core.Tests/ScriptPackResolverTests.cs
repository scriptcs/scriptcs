using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq.Protected;
using ScriptCs.Contracts;
using Xunit;
using Should;
using System.ComponentModel.Composition.Hosting;
using Moq;

namespace ScriptCs.Tests
{
    public class ScriptPackResolverTests
    {
        public class TheGetPacksMethod
        {
            private ScriptPackResolver _packManager;
            private TestExportProvider _exportProvider;
            private Mock<IScriptPack> _pack1;
            private Mock<IScriptPack> _pack2;
            
            public TheGetPacksMethod()
            {
                _pack1 = new Mock<IScriptPack>();
                _pack2 = new Mock<IScriptPack>();
                _exportProvider = new TestExportProvider(_pack1.Object, _pack2.Object);
                _packManager = new ScriptPackResolver(_exportProvider);
            }

            [Fact]
            public void ShouldRetrieveAnyPacksInTheCatalog()
            {
                var packs = _packManager.GetPacks();
                packs.Count().ShouldEqual(2);
                packs.ShouldContain(_pack1.Object);
                packs.ShouldContain(_pack2.Object);
            }

        }

        public class TestExportProvider : ExportProvider
        {
            private readonly IScriptPack[] _packs;

            public TestExportProvider(params IScriptPack[] packs)
            {
                _packs = packs;
            }

            protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
            {
                return _packs.Select(p => new Export(AttributedModelServices.GetContractName(typeof(IScriptPack)), ()=>p));
            }
        }
    }
}
