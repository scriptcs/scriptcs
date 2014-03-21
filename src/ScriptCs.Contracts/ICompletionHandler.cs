using System;

namespace ScriptCs.Contracts
{
    public interface ICompletionHandler
    {
        void UpdateBufferWithCompletion(Func<string, string[]> getPaths);
        void UpdateBufferWithPrevious();
        void Abort();
        void Reset();
    }
}