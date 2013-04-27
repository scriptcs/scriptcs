using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;
using ScriptCs.Exceptions;

namespace ScriptCs.Engine.Roslyn
{
    public class RoslynScriptEngine : IScriptEngine
    {
        private readonly ScriptEngine _scriptEngine;
        private readonly IScriptHostFactory _scriptHostFactory;

        private const string CompiledScriptClass = "Submission#0";
        private const string CompiledScriptMethod = "<Factory>";

        public RoslynScriptEngine(IScriptHostFactory scriptHostFactory)
        {
            _scriptEngine = new ScriptEngine();
            _scriptEngine.AddReference(typeof(ScriptExecutor).Assembly);
            _scriptHostFactory = scriptHostFactory;
        }

        public string BaseDirectory
        {
            get {  return _scriptEngine.BaseDirectory;  }
            set {  _scriptEngine.BaseDirectory = value; }
        }

        public bool CacheAssembly { get; set; }
        public DateTime? AssemblyCacheDate { get; set; }
        public string AssemblyName { get; set; }

        public void Execute(string code, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts));
  
            var session = _scriptEngine.CreateSession(host);

            foreach (var reference in references.Union(scriptPackSession.References).Distinct())
                session.AddReference(reference);

            foreach (var @namespace in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                session.ImportNamespace(@namespace);

            Execute(code, session);
        }
        public void ExecuteCached(IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession)
        {
            var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts));
            var session = _scriptEngine.CreateSession(host);

            foreach (var reference in references.Union(scriptPackSession.References).Distinct())
                session.AddReference(reference);

            foreach (var @namespace in namespaces.Union(scriptPackSession.Namespaces).Distinct())
                session.ImportNamespace(@namespace);

            ExecuteCached(session);
        }

        public bool CanExecuteCached()
        {
            return !NeedCompile(AssemblyCacheDate, AssemblyName);
        }
        public void CleanUpCachedAssembly()
        {
            var exeFilename = Path.Combine(BaseDirectory, AssemblyName + ".dll");
            var pdbFilename = Path.Combine(BaseDirectory, AssemblyName + ".pdb");

            if (File.Exists(exeFilename))
                File.Delete(exeFilename);

            if (File.Exists(pdbFilename))
                File.Delete(pdbFilename);
        }

        protected bool NeedCompile(DateTime? assemblyCacheDate, string assemblyName)
        {
            var exeFilename = Path.Combine(BaseDirectory, assemblyName + ".dll");
            var needCompile	= true;

            if (File.Exists(exeFilename))
            {
                if (assemblyCacheDate != null)
                {
                    var fileInfo = new FileInfo(exeFilename);
                    needCompile = fileInfo.LastWriteTime < assemblyCacheDate.GetValueOrDefault();
                }
            }

            return needCompile;
        }

        protected void ExecuteCached(Session session)
        {
            var exeFilename = Path.Combine(BaseDirectory, AssemblyName + ".dll");
            var pdbFilename = Path.Combine(BaseDirectory, AssemblyName + ".pdb");

            var exeBytes = File.ReadAllBytes(exeFilename);
            var pdbBytes = new byte[0];

            if (File.Exists(pdbFilename))
                pdbBytes = File.ReadAllBytes(pdbFilename);

            AppDomain.CurrentDomain.AssemblyResolve += ExecuteCached_AssemblyResolve;

            var assembly = AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
            var type = assembly.GetType(CompiledScriptClass);
            var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

            try
            {
                method.Invoke(null, new[] { session });
            }
            catch (Exception e)
            {
                var message = 
                    string.Format(
                    "Exception Message: {0} {1}Stack Trace:{2}",
                    e.InnerException.Message,
                    Environment.NewLine, 
                    e.InnerException.StackTrace);
                throw new ScriptExecutionException(message);
            }
        }
        protected Assembly ExecuteCached_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] tokens = args.Name.Split(",".ToCharArray());
            return Assembly.LoadFile(Path.Combine(new string[]{ BaseDirectory, tokens[0] + ".dll" }));
        }

        protected virtual void Execute(string code, Session session)
        {
            if (CacheAssembly && !String.IsNullOrEmpty(AssemblyName))
                CompileAndExecute(code, CacheAssembly, AssemblyCacheDate, AssemblyName, session);
            else
                session.Execute(code);
        }

        protected virtual void CompileAndExecute(string code, bool cacheAssembly, DateTime? assemblyCacheDate, string assemblyName, Session session)
        {
            var submission = session.CompileSubmission<object>(code);
            var exeBytes = new byte[0];
            var pdbBytes = new byte[0];
            var exeFilename = cacheAssembly && !String.IsNullOrEmpty(assemblyName) ? Path.Combine(BaseDirectory, assemblyName + ".dll") : null;
            var pdbFilename = cacheAssembly && !String.IsNullOrEmpty(assemblyName) ? Path.Combine(BaseDirectory, assemblyName + ".pdb") : null;
            var needCompile = true;
            var compileSuccess = false;

            if (cacheAssembly)
            {
                needCompile = NeedCompile(assemblyCacheDate, assemblyName);
                compileSuccess = !needCompile;
            }

            if (needCompile)
            {
                using (var exeStream = new MemoryStream())
                using (var pdbStream = new MemoryStream())
                {
                    var result = submission.Compilation.Emit(exeStream, pdbStream: pdbStream);
                    compileSuccess = result.Success;

                    if (result.Success)
                    {
                        exeBytes = exeStream.ToArray();
                        pdbBytes = pdbStream.ToArray();

                        if (cacheAssembly)
                        {
                            if (File.Exists(exeFilename)) { Console.WriteLine("Deleting " + exeFilename);
                                File.Delete(exeFilename); }
                            File.WriteAllBytes(exeFilename, exeBytes);

                            if (File.Exists(pdbFilename))
                                File.Delete(pdbFilename);
                            File.WriteAllBytes(pdbFilename, pdbBytes);
                        }
                    }
                    else
                    {
                        var errors = String.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                        Console.WriteLine(errors);
                    }
                }
            }
            else
            {
                exeBytes = File.ReadAllBytes(exeFilename);

                if (File.Exists(pdbFilename))
                    pdbBytes = File.ReadAllBytes(pdbFilename);

                compileSuccess = true;
            }

            if (compileSuccess)
            {
                var assembly = AppDomain.CurrentDomain.Load(exeBytes, pdbBytes);
                var type = assembly.GetType(CompiledScriptClass);
                var method = type.GetMethod(CompiledScriptMethod, BindingFlags.Static | BindingFlags.Public);

                try
                {
                    method.Invoke(null, new[] { session });
                }
                catch (Exception e)
                {
                    var message = 
                        string.Format(
                        "Exception Message: {0} {1}Stack Trace:{2}",
                        e.InnerException.Message,
                        Environment.NewLine, 
                        e.InnerException.StackTrace);
                    throw new ScriptExecutionException(message);
                }
            }
        }
    }
}
