using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
    public class CSharpScriptEngine : CommonScriptEngine
    {
        public CSharpScriptEngine(IScriptHostFactory scriptHostFactory, ILogProvider logProvider) : base(scriptHostFactory, logProvider)
        {
        }


        protected override ScriptState GetScriptState(string code, object globals)
        {
           return CSharpScript.Run(code, ScriptOptions, globals);
        }

        protected bool IsCompleteSubmission(string code)
        {
            //invalid REPL command
            if (code.StartsWith(":"))
            {
                return true;
            }

            var options = new CSharpParseOptions(LanguageVersion.CSharp6, DocumentationMode.Parse,
                SourceCodeKind.Interactive, null);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options: options);
            return SyntaxFactory.IsCompleteSubmission(syntaxTree);
        }
    }
}