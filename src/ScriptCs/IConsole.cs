﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public interface IConsole
    {
        void Write(string value);
        void WriteLine(string value);
        string ReadLine();
        ConsoleColor ForegroundColor { get; set; }
    }
}


