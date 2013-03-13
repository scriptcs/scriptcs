using System.Collections.Generic;
using System.Linq;

namespace ScriptCs
{
    public class DebugFilePreProcessor : FilePreProcessor
    {
        private const string SystemDiagnosticsUsing = "using System.Diagnostics;";

        public DebugFilePreProcessor(IFileSystem fileSystem)
            : base(fileSystem)
        {
        }

        protected override string GenerateUsings(ICollection<string> usingLines)
        {
            if (usingLines.Count(l => l.Equals(SystemDiagnosticsUsing)) == 0)
            {
                usingLines.Add(SystemDiagnosticsUsing);
            }
            
            return string.Join(_fileSystem.NewLine, usingLines.Distinct());
        }
    }
}
