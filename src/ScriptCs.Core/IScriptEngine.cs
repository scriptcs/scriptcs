using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
	[InheritedExport]
	public interface IScriptEngine
	{
		string BaseDirectory { get; set; }
		void Execute(string code, IEnumerable<string> references, IScriptPackSession scriptPackSession, object hostObject = null);
	}
}