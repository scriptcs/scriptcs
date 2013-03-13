using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Roslyn
{
	[InheritedExport]
	public interface IScriptHostFactory
	{
		ScriptHost CreateScriptHost(IEnumerable<IScriptPackContext> contexts);
	}
}
