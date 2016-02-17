using System;
using System.Collections.Generic;
using ScriptCs.Contracts;

namespace ScriptCs.Contracts
{
    public interface IVisualStudioSolutionWriter
    {
        string WriteSolution(IFileSystem fs, string script, IVisualStudioSolution solution, IList<ProjectItem> nestedItems = null);
    }
}