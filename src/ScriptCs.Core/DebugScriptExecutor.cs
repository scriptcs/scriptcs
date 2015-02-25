using Common.Logging;

using ScriptCs.Contracts;

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
