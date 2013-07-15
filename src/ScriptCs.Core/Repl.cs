﻿using Common.Logging;
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

        public string Buffer { get; set; }

        public override ScriptResult Execute(string script)
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
                    var assemblyName = PreProcessorUtil.GetPath(PreProcessorUtil.RString, script);
                    var assemblyPath = FileSystem.GetFullPath(Path.Combine(Constants.BinFolder, assemblyName));
                    AddReference(FileSystem.FileExists(assemblyPath) ? assemblyPath : assemblyName);

                    return new ScriptResult();
                }

                Console.ForegroundColor = ConsoleColor.Cyan;

                Buffer += script;

                var result = ScriptEngine.Execute(Buffer, new string[0], References, DefaultNamespaces, ScriptPackSession);
                if (result != null)
                {
                    if (result.CompileException != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(result.CompileException.ToString());
                    }

                    if (result.ExecuteException != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(result.ExecuteException.ToString());
                    }

                    if (result.IsPendingClosingChar)
                    {
                        return result;
                    }
                    else
                    {
                        Buffer = null;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(result.ReturnValue.ToJsv());
                    }
                }

                return result;
            }
            catch (FileNotFoundException fileEx)
            {
                RemoveReference(fileEx.FileName);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n" + fileEx + "\r\n");
                return new ScriptResult { CompileException = fileEx };
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\r\n" + ex + "\r\n");
                return new ScriptResult { ExecuteException = ex };
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }
}
