using System.ComponentModel.Composition;

namespace ScriptCs
{
	[Export(Constants.DebugContractName, typeof(IScriptExecutor))]
	public class DebugScriptExecutor : ScriptExecutor
	{
		[ImportingConstructor]
		public DebugScriptExecutor(IFileSystem fileSystem, [Import(Constants.DebugContractName)]IFilePreProcessor filePreProcessor, [Import(Constants.DebugContractName)]IScriptEngine scriptEngine, IScriptHostFactory scriptHostFactory)
			: base(fileSystem, filePreProcessor, scriptEngine, scriptHostFactory) { }
	}
}