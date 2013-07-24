using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs.Contracts
{
	public interface IScriptHostFactory
	{
		ScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs);
	}
}
