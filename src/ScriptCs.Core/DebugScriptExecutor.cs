using Common.Logging;

namespace ScriptCs
{
    public class DebugScriptExecutor : ScriptExecutor
    {
        public DebugScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, ILog logger)
            : base(fileSystem, filePreProcessor, scriptEngine, logger)
        {
        }
    }
}
