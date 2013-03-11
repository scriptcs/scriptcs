using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting.CSharp;
using Scriptcs.Contracts;

namespace Scriptcs
{
	[Export("-debug", typeof(IScriptExecutor))]
	public class ScriptDebugger : IScriptExecutor
	{
		private readonly IFileSystem _fileSystem;

		private const string TypeName = "Submission#0";
		private const string FactoryMethodName = "<Factory>";

		private const string AttachDebuggerMessage = "To debug, attach your debugger to process {0} and press ENTER in this command window.";

		[ImportingConstructor]
		public ScriptDebugger(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptcsRecipe> recipes)
		{
			var scriptName = Path.GetFileNameWithoutExtension(script);

			var engine = new ScriptEngine();
			engine.AddReference("mscorlib");
			engine.AddReference("System");
			engine.AddReference("System.Core");

			var bin = _fileSystem.CurrentDirectory + @"\bin";
			engine.BaseDirectory = bin;

			var outputDll = Path.Combine(bin, scriptName + ".dll");
			var outputPdb = Path.Combine(bin, scriptName + ".pdb");

			if (!_fileSystem.DirectoryExists(bin))
				_fileSystem.CreateDirectory(bin);

			foreach (var file in paths) {
				var destFile = bin + @"\" + Path.GetFileName(file);
				var sourceFileLastWriteTime = _fileSystem.GetLastWriteTime(file);
				var destFileLastWriteTime = _fileSystem.GetLastWriteTime(destFile);
				if (sourceFileLastWriteTime != destFileLastWriteTime)
					_fileSystem.Copy(file, destFile, true);

				engine.AddReference(destFile);
			}

			var session = engine.CreateSession();
			var csx = _fileSystem.ReadFile(_fileSystem.CurrentDirectory + @"\" + script);
			var submission = session.CompileSubmission<object>(csx);

			var startDebuggerProcess = false;
			var dllBytes = new byte[0];
			var pdbBytes = new byte[0];

			using (var dllStream = new MemoryStream())
			using (var pdbStream = new MemoryStream()) {
				var result = submission.Compilation.Emit(
					executableStream: dllStream,
					pdbStream: pdbStream);

				startDebuggerProcess = result.Success;

				if (!result.Success) {
					var error = String.Join(Environment.NewLine, result.Diagnostics.Select(d => d.ToString()));
					Console.WriteLine("failed to compile");
					Console.WriteLine(error);
				} else {
					dllBytes = dllStream.ToArray();
					pdbBytes = pdbStream.ToArray();
				}
			}

			if (startDebuggerProcess) {
				var assembly = AppDomain.CurrentDomain.Load(dllBytes, pdbBytes);
				var type = assembly.GetType(TypeName);
				var factory = type.GetMethod(FactoryMethodName, BindingFlags.Static | BindingFlags.Public);

				var processId = Process.GetCurrentProcess().Id;
				Console.WriteLine(new String('-', 10));
				Console.WriteLine(AttachDebuggerMessage, processId);
				Console.WriteLine(new String('-', 10));
				Console.ReadLine();

				factory.Invoke(null, new[] { session });
			}
		}
	}
}
