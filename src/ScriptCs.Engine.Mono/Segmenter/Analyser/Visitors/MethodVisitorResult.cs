using ICSharpCode.NRefactory.CSharp;

namespace ScriptCs.Engine.Mono.Segmenter.Analyser.Visitors
{
    public class MethodVisitorResult
    {
        public MethodDeclaration MethodDefinition { get; set; }

        public VariableDeclarationStatement MethodPrototype { get; set; }
        
        public ExpressionStatement MethodExpression { get; set; }
    }
}