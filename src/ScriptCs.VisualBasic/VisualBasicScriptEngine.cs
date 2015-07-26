using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic;
using ScriptCs.Contracts;
using ScriptCs.Engine.Common;
using ScriptCs.Logging;

namespace ScriptCs.VisualBasic
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
            var diagnostics = syntaxTree.GetDiagnostics();
            return !diagnostics.Any();
        }
    }
}