namespace ScriptCs.Contracts
{
    using System;
    using System.Collections.Generic;

    public interface IModuleConfiguration : IServiceOverrides<IModuleConfiguration>
    {
        bool Cache { get; }

        string ScriptName { get; }

        bool Repl { get; }

        LogLevel LogLevel { get; }

        IEnumerable<Type> CodeRewriters { get; }

        IEnumerable<Type> LineProcessors { get; }
    }
}