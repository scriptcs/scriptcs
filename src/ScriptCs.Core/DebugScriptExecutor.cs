namespace ScriptCs
{
    public class DebugScriptExecutor : ScriptExecutor
    {
        public DebugScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, IScriptHostFactory scriptHostFactory)
            : base(fileSystem, filePreProcessor, scriptEngine, scriptHostFactory)
        {
        }

        public DebugScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine)
            : base(fileSystem, filePreProcessor, scriptEngine)
        {
        }
    }
}
