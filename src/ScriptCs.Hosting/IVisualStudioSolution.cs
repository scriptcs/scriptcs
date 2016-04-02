using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IVisualStudioSolution
    {
        void AddHeader();
        void AddScriptcsProject(string scriptcsPath, string workingPath, string args, bool attach, Guid projectGuid);
        void AddProject(string path, string name, Guid guid, string[] files);
        void AddGlobalHeader(Guid projectGuid);
        void AddGlobalNestedProjects(IList<ProjectItem> nestedItems);
        void AddGlobal(Guid projectGuid, IList<ProjectItem> nestedItems);
    }
}