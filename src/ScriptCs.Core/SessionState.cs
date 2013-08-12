using System.Collections.Generic;

namespace ScriptCs
{
    public class SessionState<T>
    {
        public T Session { get; set; }
        public IEnumerable<string> References { get; set; }
        public ScriptEnvironment Environment { get; set; }
    }
}