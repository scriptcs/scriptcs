using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
	public interface IScriptEngine
	{
		string BaseDirectory { get; set; }
		void Execute(string code, IEnumerable<string> references, IScriptPackSession scriptPackSession, object hostObject = null);
	}
}