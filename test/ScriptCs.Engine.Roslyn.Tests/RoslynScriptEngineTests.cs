using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using Xunit;

namespace ScriptCs.Tests
{
    public class RoslynScriptEngineTests
    {
        private static RoslynScriptEngine CreateScriptEngine(
            Mock<IScriptHostFactory> scriptHostFactory = null)
        {
            scriptHostFactory = scriptHostFactory ?? new Mock<IScriptHostFactory>();

            return new RoslynScriptEngine();
        }

        public class TheExecuteMethod 
        {

        }
    }
}