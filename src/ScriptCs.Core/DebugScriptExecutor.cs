using ScriptCs.Contracts;
using ScriptCs.Contracts.Logging;

namespace ScriptCs
{
    public class DebugScriptExecutor : ScriptExecutor
    {
        public DebugScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, ILog logger, IScriptLibraryComposer composer)
            : base(fileSystem, filePreProcessor, scriptEngine, logger, composer)
        {
        }
    }
}
