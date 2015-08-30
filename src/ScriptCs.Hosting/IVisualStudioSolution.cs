using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IVisualStudioSolution
    {
        void AddHeader();
        void AddScriptcsProject(string scriptcsPath, string workingPath, string args, bool attach, string projectGuid);
        void AddProject(string path, string name, string guid, string[] files);
        void AddGlobalHeader(string projectGuid);
        void AddGlobalNestedProjects(IList<Tuple<string, string>> nestedItems);
        void AddGlobal(string projectGuid, IList<Tuple<string, string>> nestedItems);
    }
}