using System.Collections.Generic;

using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
	public interface IScriptHostFactory
	{
		ScriptHost CreateScriptHost(IScriptPackManager scriptPackManager);
	}
}
