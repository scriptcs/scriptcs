using System.Linq;
using Common.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic;
using ScriptCs.Contracts;

namespace ScriptCs.CSharp
{
    public class VisualBasicScriptEngine : CommonScriptEngine
    {
        public VisualBasicScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
            : base(scriptHostFactory, logger)
        {
        }

        protected override ScriptState GetScriptState(string code, object globals)
        {
            return VisualBasicScript.Run(code, ScriptOptions, globals);
        }

        protected bool IsCompleteSubmission(string code)
        {
            //invalid REPL command
            if (code.StartsWith(":"))
            {
                return true;
            }

            var options = new VisualBasicParseOptions(LanguageVersion.VisualBasic14, DocumentationMode.Parse, SourceCodeKind.Interactive, null);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options);
            return !syntaxTree.GetDiagnostics().Any();
        }
    }
}