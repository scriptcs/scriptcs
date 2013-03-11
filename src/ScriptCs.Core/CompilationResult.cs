using System.Collections.Generic;
using System.Linq;
using Roslyn.Compilers.Common;

namespace ScriptCs
{
    using System;

    public class CompilationResult : ICompilationResult
    {
        public CompilationResult(CommonEmitResult emitResult)
        {
            Success = emitResult.Success;
            ErrorMessage = RetrieveErrorMessage(emitResult.Diagnostics);
        }

        public bool Success { get; private set; }

        public string ErrorMessage { get; private set; }

        private string RetrieveErrorMessage(IEnumerable<CommonDiagnostic> diagnostics)
        {
            return string.Join(Environment.NewLine, diagnostics.Select(d => d.ToString()));
        }
    }
}
