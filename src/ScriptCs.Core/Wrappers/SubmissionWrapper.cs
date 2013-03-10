using Roslyn.Compilers.CSharp;
using Roslyn.Scripting;

namespace ScriptCs.Wrappers
{
    public class SubmissionWrapper<T> : ISubmission<T>
    {
        private readonly Submission<T> _submission;

        public SubmissionWrapper(Submission<T> submission)
        {
            this._submission = submission;
        }

        public ICompilation Compilation 
        { 
            get
            {
                return new CompilationWrapper((Compilation)this._submission.Compilation);
            }
        }
    }
}