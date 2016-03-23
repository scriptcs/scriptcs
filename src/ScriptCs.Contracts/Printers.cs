using System;
using System.Collections.Generic;

namespace ScriptCs.Contracts
{
    public class Printers : Dictionary<Type, Func<object, string>>
    {
    }
}