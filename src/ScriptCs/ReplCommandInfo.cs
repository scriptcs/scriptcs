using System;

namespace ScriptCs
{
    public interface IReplCommand
    {
        string Name { get; }
        string HelpName { get; }
        string HelpDescription { get; }

        object Execute();
    }
}