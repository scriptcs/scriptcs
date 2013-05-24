using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs
{
	public interface IScriptHostFactory
	{
		ScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs);
	}
}
