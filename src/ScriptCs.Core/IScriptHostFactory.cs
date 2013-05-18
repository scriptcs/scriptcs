namespace ScriptCs
{
	public interface IScriptHostFactory
	{
		ScriptHost CreateScriptHost(string scriptArgs, IScriptPackManager scriptPackManager);
	}
}
