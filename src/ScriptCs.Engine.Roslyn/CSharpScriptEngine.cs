using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
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
           return CSharpScript.RunAsync(code, ScriptOptions, globals).GetAwaiter().GetResult();
        }

        protected bool IsCompleteSubmission(string code)
        {
            //invalid REPL command
            if (code.StartsWith(":"))
            {
                return true;
            }

            var options = new CSharpParseOptions(LanguageVersion.CSharp6, DocumentationMode.Parse, SourceCodeKind.Script);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code, options: options);
            return SyntaxFactory.IsCompleteSubmission(syntaxTree);
        }
    }
}