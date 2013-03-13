using System;
using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyProduct("scriptcs")]
[assembly: AssemblyCopyright("Copyright 2013 Glenn Block, Justin Rusbatch, Filip Wojcieszyn")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]