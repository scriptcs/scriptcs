using ScriptCs.Contracts;
using System;

namespace ScriptCs
{
    public class CompilationExceptionWriter : ICompilationExceptionWriter
    {
        public string CompilationExceptionFilePath
        {
            get
            {
                return System.Environment.CurrentDirectory + "\\" + "cex_" + DateTime.UtcNow.ToString("YYYYddMM") + ".txt";
            }
        }
    }
}
