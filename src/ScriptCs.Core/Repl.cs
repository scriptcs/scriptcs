using Common.Logging;
using ScriptCs.Contracts;
using ServiceStack.Text;
using System;
using System.IO;
using System.Linq;

namespace ScriptCs
{
    public class Repl : ScriptExecutor
    {
        public IConsole Console { get; private set; }

        public Repl(IFileSystem fileSystem, IScriptEngine scriptEngine, ILog logger, IConsole console, IFilePreProcessor filePreProcessor)
            : base(fileSystem, filePreProcessor, scriptEngine, logger)
        {
            Console = console;
        }

        public override void Terminate()
        {
            base.Terminate();
            Logger.Debug("Exiting console");
            Console.Exit();
        }

        public override void Execute(string script)
        {
            try
            {
                if (PreProcessorUtil.IsLoadLine(script))
                {
                    var filepath = PreProcessorUtil.GetPath(PreProcessorUtil.LoadString, script);
                    if (FileSystem.FileExists(filepath))
                    {
                        var processorResult = FilePreProcessor.ProcessFile(filepath);
                        script = processorResult.Code;
                    }
                    else
                    {
                        throw new FileNotFoundException(string.Format("Could not find script '{0}'", filepath), filepath);
                    }
                }
                else if (PreProcessorUtil.IsRLine(script))
                {
                    var assemblyPath = FileSystem.GetFullPath(Path.Combine(Constants.BinFolder, PreProcessorUtil.GetPath(PreProcessorUtil.RString, script)));
                    if (FileSystem.FileExists(assemblyPath))
                    {
                        References = References.Union(new[] { assemblyPath });
                    }
                    else
                    {
                        throw new FileNotFoundException(string.Format("Could not find assembly '{0}'", assemblyPath), assemblyPath);
                    }

                    return;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                var result = ScriptEngine.Execute(script, new string[0], References, DefaultNamespaces, ScriptPackSession);
                if (result != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(result.ToJsv());
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n" + ex + "\r\n");
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
