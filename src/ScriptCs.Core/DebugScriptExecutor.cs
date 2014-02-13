using Common.Logging;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class DebugScriptExecutor : ScriptExecutor
    {
        public DebugScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, IScriptHostFactory hostFactory, ILog logger)
            : base(fileSystem, filePreProcessor, scriptEngine, hostFactory, logger)
        {
        }
    }
}
