namespace ScriptCs.Engine.Mono.Parser
{
    using System.Collections.Generic;

    public class ParseResult
    {
        public IEnumerable<string> TypeDeclarations { get; set; }
        public IEnumerable<string> MethodPrototypes { get; set; }
        public IEnumerable<string> MethodExpressions { get; set; }
        public string Evaluations { get; set; }
    }
}