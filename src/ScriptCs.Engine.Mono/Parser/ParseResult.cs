namespace ScriptCs.Engine.Mono.Parser
{
    using System.Collections.Generic;

    public class ParseResult
    {
        public string TypeDeclarations { get; set; }
        public List<string> MethodPrototypes { get; set; }
        public List<string> MethodExpressions { get; set; }
        public string Evaluations { get; set; }
    }
}