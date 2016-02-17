using System;

namespace ScriptCs.Contracts
{
    public class ProjectItem
    {
        public ProjectItem(Guid project, Guid parent)
        {
            Project = project;
            Parent = parent;
        }

        public Guid Parent { get; private set; }
        public Guid Project { get; private set; }
    }
}