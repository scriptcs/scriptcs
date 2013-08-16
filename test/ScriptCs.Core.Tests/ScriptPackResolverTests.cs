using System.Linq;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptPackResolverTests
    {
        public class TheGetPacksMethod
        {
            private ScriptPackResolver _packManager;
            private Mock<IScriptPack> _pack1;
            private Mock<IScriptPack> _pack2;
            
            public TheGetPacksMethod()
            {
                _pack1 = new Mock<IScriptPack>();
                _pack2 = new Mock<IScriptPack>();
                var packs = new[] { _pack1.Object, _pack2.Object };
                _packManager = new ScriptPackResolver(packs);
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
    }
}
