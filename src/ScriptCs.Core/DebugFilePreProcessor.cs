using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace ScriptCs
{
    [Export(Constants.DebugContractName, typeof(IFilePreProcessor))]
    public class DebugFilePreProcessor : FilePreProcessor
    {
        private const string SystemDiagnosticsUsing = "using System.Diagnostics;";

        [ImportingConstructor]
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
