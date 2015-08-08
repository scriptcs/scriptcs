using System;
using ScriptCs.Contracts;

namespace ScriptCs
{
    public class DebugScriptExecutor : ScriptExecutor
    {
        [Obsolete("Support for Common.Logging types was deprecated in version 0.15.0 and will soon be removed.")]
        public DebugScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, Common.Logging.ILog logger, IScriptLibraryComposer composer)
            : this(fileSystem, filePreProcessor, scriptEngine, new CommonLoggingLogProvider(logger), composer)
        {
        }

        public DebugScriptExecutor(IFileSystem fileSystem, IFilePreProcessor filePreProcessor, IScriptEngine scriptEngine, ILogProvider logProvider, IScriptLibraryComposer composer)
            : base(fileSystem, filePreProcessor, scriptEngine, logProvider, composer)
        {
        }
    }
}
