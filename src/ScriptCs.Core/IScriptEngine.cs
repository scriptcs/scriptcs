﻿using System.Collections.Generic;

namespace ScriptCs
{
    public interface IScriptEngine
    {
        string BaseDirectory { get; set; }
        object Execute(string code, string args, IEnumerable<string> references, IEnumerable<string> namespaces, ScriptPackSession scriptPackSession);
    }
}