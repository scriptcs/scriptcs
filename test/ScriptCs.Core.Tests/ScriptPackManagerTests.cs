using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;
using Moq;
using ScriptCs.Contracts;
using Should;
using Xunit;

namespace ScriptCs.Tests
{
    public class ScriptPackManagerTests
    {
        public class TheConstructor
        {
            private Dictionary<Type, IScriptPackContext> _contextLookup;
            private ScriptPackManager _manager;
            private IScriptPackContext _context;

            public TheConstructor()
            {
                _context = new FakeContext();
                _contextLookup = new Dictionary<Type, IScriptPackContext>();
                _manager = new ScriptPackManager(new List<IScriptPackContext> {_context}, _contextLookup);
            }


            [Fact]
            public void ShouldInitializeTheInternalContextLookupKeyedOffOfType()
            {
                _contextLookup[_context.GetType()].ShouldEqual(_context);
            }
        }

        public class TheGetOfTContextMethod
        {
            private Dictionary<Type, IScriptPackContext> _contextLookup;
            private ScriptPackManager _manager;
            private IScriptPackContext _context;

            public TheGetOfTContextMethod()
            {
                _context = new FakeContext();
                _contextLookup = new Dictionary<Type, IScriptPackContext>();
                _manager = new ScriptPackManager(new List<IScriptPackContext> { _context }, _contextLookup);
            }

            [Fact]
            public void ShouldRetrieveTheContextForTheType()
            {
                _manager.Get<FakeContext>().ShouldEqual(_context);
            }
        }

        public class TheGetMethod
        {
            private Dictionary<Type, IScriptPackContext> _contextLookup;
            private ScriptPackManager _manager;
            private IScriptPackContext _context;

            public TheGetMethod()
            {
                _context = new FakeContext();
                _contextLookup = new Dictionary<Type, IScriptPackContext>();
                _manager = new ScriptPackManager(new List<IScriptPackContext> { _context }, _contextLookup);
            }

            [Fact]
            public void ShouldRetrieveTheContextForTheType()
            {
                _manager.Get(typeof(FakeContext)).ShouldEqual(_context);
            }

            [Fact]
            public void ShouldRetrieveNullIfTheContextTypeDoesNotExist()
            {
                _contextLookup.Clear();
                _manager.Get(typeof(FakeContext)).ShouldBeNull();
            }
        }

        public class FakeContext : IScriptPackContext
        {
            
        }


    }
}
