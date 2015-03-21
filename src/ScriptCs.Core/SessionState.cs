using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class SessionState<T>
    {
        public T Session { get; set; }

#pragma warning disable 618
        public AssemblyReferences References { get; set; }
#pragma warning restore 618

        public HashSet<string> Namespaces { get; set; } 
    }
}