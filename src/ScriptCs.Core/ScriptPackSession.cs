using System;
using System.Collections.Generic;
using System.Linq;
using ScriptCs.Contracts;

namespace ScriptCs
{
	public class ScriptPackSession : IScriptPackSession, IDisposable
	{
		private readonly IEnumerable<IScriptPack> _scriptPacks;

		public ScriptPackSession(IEnumerable<IScriptPack> scriptPacks)
		{
			_scriptPacks = scriptPacks;

			References = new List<string>();
			Namespaces = new List<string>();

			InitializePacks();
		}

		public List<string> References { get; private set; }
		public List<string> Namespaces { get; private set; }

		public void Dispose()
		{
			TerminatePacks();
		}

		private void InitializePacks()
		{
			foreach (var s in _scriptPacks)
				s.Initialize(this);
		}

		private void TerminatePacks()
		{
			foreach (var s in _scriptPacks)
				s.Terminate();
		}

		void IScriptPackSession.AddReference(string assemblyDisplayNameOrPath)
		{
			References.Add(assemblyDisplayNameOrPath);
		}

		void IScriptPackSession.ImportNamespace(string @namespace)
		{
			Namespaces.Add(@namespace);
		}
	}
}
