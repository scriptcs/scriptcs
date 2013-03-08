Notes
=====
* Using [this version](http://www.microsoft.com/en-us/download/details.aspx?id=19621) of mdbg. Otherwise the error *Error: The debugger's protocol is incompatible with the debuggee. (Exception from HRESULT: 0x8013134B)* is thrown (at least by WinSDK 7.1 mdbg).
* To generate a console app .exe and .pdb run: 
```
DebugSymbols [.csx file]
```
* Important: In this spike the .csx file must only make use of classes in mscorlib, System and System.Core assemblies. They are hardcoded references. Of course, we would need to reference all nuget packages.

Findings
========
To compile SyntaxTree is being used, could not find a way to build from ScriptEngine. Generating a .pdb file with the current settings seems simple. The following figure shows a debugging session with mdbg (still need to figure out how to get mdbg to recognize the source file).
![alt text][logo]

[logo]: https://github.com/dschenkelman/scriptcs/blob/dev/spikes/DebugSymbols/debugSession.png "Debugging session"

Source needs to be a valid "compilation unit". In the case of a console app, that means having an entry point (main method) and inside a class. For example, this code:
```
using System;
Console.WriteLine("Attach and the press ENTER to start");
Console.ReadLine();
int a = 1;
int b = 2;
int c = a + b;
Console.WriteLine(c);
Console.ReadLine();
```
Throws the following:
* (4,9): error CS0116: A namespace does not directly contain members such as fields or methods
* (5,9): error CS0116: A namespace does not directly contain members such as fields or methods
* (4,18): error CS1022: Type or namespace definition, or end-of-file expected
* (5,17): error CS1022: Type or namespace definition, or end-of-file expected
* (11,9): error CS0116: A namespace does not directly contain members such as fields or methods
* (11,18): error CS1022: Type or namespace definition, or end-of-file expected
* (11,19): error CS0116: A namespace does not directly contain members such as fields or methods
* (11,20): error CS1022: Type or namespace definition, or end-of-file expected
* (12,9): error CS0116: A namespace does not directly contain members such as fields or methods
* (12,17): error CS1022: Type or namespace definition, or end-of-file expected
* (7,5): error CS0116: A namespace does not directly contain members such as fields or methods
* (8,5): error CS0116: A namespace does not directly contain members such as fields or methods
* (10,5): error CS0116: A namespace does not directly contain members such as fields or methods
* (10,9): error CS0103: The name 'a' does not exist in the current context
* (10,13): error CS0103: The name 'b' does not exist in the current context
* error CS5001: Program does not contain a static 'Main' method suitable for an     entry point
    
But this other fragment does work:
```
using System;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Attach and the press ENTER to start");
        Console.ReadLine();
        int a = 1;
        int b = 2;
        int c = a + b;
        Console.WriteLine(c);
        Console.ReadLine();
    }
}
``` 
