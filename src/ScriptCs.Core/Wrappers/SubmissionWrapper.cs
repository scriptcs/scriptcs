using Roslyn.Scripting;

namespace ScriptCs.Wrappers
{
    using System.Threading;

    using Roslyn.Compilers;

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
                return new CompilationWrapper(this._submission.Compilation);
            }
        }
    }
}
