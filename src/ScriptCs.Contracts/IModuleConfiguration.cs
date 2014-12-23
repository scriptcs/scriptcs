using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public interface IModuleConfiguration : IServiceOverrides<IModuleConfiguration>
    {
        bool Cache { get; }

        string ScriptName { get; }

        bool IsRepl { get; }

        LogLevel LogLevel { get; }

        bool Debug { get; }

        IDictionary<Type, object> Overrides { get; }
    }
}
