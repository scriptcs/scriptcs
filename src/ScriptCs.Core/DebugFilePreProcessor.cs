using System.ComponentModel.Composition;
using System.Linq;

namespace ScriptCs
{
    [Export(Constants.DebugContractName, typeof(IFilePreProcessor))]
    public class DebugFilePreProcessor : FilePreProcessor
    {
        private const string SystemDiagnosticsUsing = "System.Diagnostics";

        [ImportingConstructor]
        public DebugFilePreProcessor(IFileSystem fileSystem)
            : base(fileSystem)
        {
        }

        protected override string GenerateScript(ParsedFileResult parsedFileResult)
        {
            if (parsedFileResult.Usings.All(@using => @using != SystemDiagnosticsUsing))
            {
                parsedFileResult.Usings.Add(SystemDiagnosticsUsing);
            }

            return base.GenerateScript(parsedFileResult);
        }
    }
}
