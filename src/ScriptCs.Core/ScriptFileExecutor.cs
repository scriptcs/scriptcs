using System.Linq;

using Common.Logging;

using ScriptCs.Contracts;

namespace ScriptCs
{
    public class ScriptFileExecutor : ScriptExecutor
    {
        public ScriptFileExecutor(
            IFileSystem fileSystem,
            IFilePreProcessor filePreProcessor,
            IScriptEngine scriptEngine,
            ILog logger) : base(fileSystem, filePreProcessor, scriptEngine, logger) { }

        protected override ScriptResult Execute(FilePreProcessorResult result, string[] scriptArgs)
        {
            var references = References.Union(result.References);
            var namespaces = Namespaces.Union(result.Namespaces);

            Logger.Debug("Starting execution in engine");
            return ScriptEngine.Execute(result.Code, scriptArgs, references, namespaces, ScriptPackSession);
        }
    }
}