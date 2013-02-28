using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Scripting.CSharp;

namespace Scriptcs.Contracts
{
    public interface IScriptcsRecipe
    {
        void ConfigureEngine(ScriptEngine engine);
    }
}
