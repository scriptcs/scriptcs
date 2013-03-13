using System.Collections.Generic;

namespace ScriptCs
{
	public interface IScriptEngine
	{
		string BaseDirectory { get; set; }
		void Execute(string code, IEnumerable<string> references, ScriptPackSession scriptPackSession);
	}
}
