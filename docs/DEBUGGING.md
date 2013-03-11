scriptcs Debugging overview
===========================
This document explains the steps required to debug .csx files with scriptcs. This document also goes over the limitations of the current debugging approach and the reasons for those limitations.

What you will be able to do
---------------------------
* Use [mdbg](http://msdn.microsoft.com/en-us/library/ms229861.aspx) to debug .csx scripts.
* Place breakpoints in your code using `Debugger.Break();`. No need to add the `using System.Diagnostics;` as it is automatically added when debugging. We are considering the usage of `#debug` instead of `Debugger.Break();` as it is less verbose. The downside is that you would lose direct portability to a VS project (feedback?).
* Compile your .csxs to .dlls.

What you won't be able to do
-----------------------------
* Use VS to debug.
* Use mdbg to get the current line of code.

The aforementioned limitations are due to the fact that:
1. The compiled code is not a 1 to 1 mapping of the .csx files.
2. The generated .pdb files do not point to the source file (due to 1.)

Debugging with scriptcs
-----------------------
The following example shows how you can debug the [WebApiHost sample](https://github.com/scriptcs/scriptcs/tree/dev/samples/webapihost) using scriptcs. This procedure assumes that you have the .csx, packages.config and Packages folder already setup. 
Additionally, you should have mdbg.exe already installed with support for .NET 4 configured in your PATH (if you have VS installed it should be under **C:\Program Files\Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools** and **C:\Program Files\Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools\x64**). This sample uses the x64 version, but you might need to use the other one (this is explained below).

1. Open server.csx and add a debugger break to the **TestController**'s **Get** method:

	```
	public string Get() {
		Debugger.Break();

		return "Hello World";
	}
	```

2. Open your two instances of your favorite command line tool (let's call them *console1* and *console2*) and cd to the directory where server.csx is located.

3. In *console1* enter `scriptcs.exe server.csx -debug`. The output should look like the following one (notice the scriptcs automatically halts before starting to run your scripts' code):
	
	```
	PS C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug> .\scriptcs.exe server.csx -debug
	Found assembly reference: C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug\packages\Microsoft.AspNet.Web
	Api.Client.4.0.20710.0\lib\net40\System.Net.Http.Formatting.dll
	Found assembly reference: C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug\packages\Microsoft.AspNet.Web
	Api.Core.4.0.20710.0\lib\net40\System.Web.Http.dll
	Found assembly reference: C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug\packages\Microsoft.AspNet.Web
	Api.SelfHost.4.0.20918.0\lib\net40\System.Web.Http.SelfHost.dll
	Found assembly reference: C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug\packages\Microsoft.Net.Http.2
	.0.20710.0\lib\net40\System.Net.Http.dll
	Found assembly reference: C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug\packages\Microsoft.Net.Http.2
	.0.20710.0\lib\net40\System.Net.Http.WebRequest.dll
	Found assembly reference: C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug\packages\Newtonsoft.Json.4.5.
	11\lib\net40\Newtonsoft.Json.dll
	Attach to process 7148 and press ENTER. Then use the 'go' command in the debugger.
	```

4. In *console2* execute `mdbg.exe` and run `processenum`. The output should look like this (the important entry is 1988, the scriptcs.exe one, which should match the number displayed by scriptcs. If it is not there, use the other version of mdbg.exe):

	```
	PS C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug> mdbg.exe
	MDbg (Managed debugger) v4.0.30319.1 (RTMRel.030319-0100) started.
	Copyright (C) Microsoft Corporation. All rights reserved.

	For information about commands type "help";
	to exit program type "quit".

	mdbg> processenum
	Active processes on current machine:
	(PID: 2156) C:\Windows\Explorer.EXE
	        v4.0.30319
	(PID: 4056) C:\WINDOWS\system32\WindowsPowerShell\v1.0\powershell.exe
	        v2.0.50727
	(PID: 6548) C:\WINDOWS\system32\WindowsPowerShell\v1.0\powershell.exe
	        v2.0.50727
	(PID: 7148) C:\Users\Damian\Documents\GitHub\scriptcs\src\Scriptcs\bin\debug\scriptcs.exe
	        v4.0.30319
	```

5. Attach to 7148 by running `attach 7148` in *console2*.
6. In *console1* press ENTER to continue start running your scripts.
7. In *console2* run `go` (it's like F5 in VS). In this case, mdbg.exe will not return the prompt as it is waiting for the breakpoint to be hit, but `Listening...` will be displayed in *console1* (among other things).
8. Open your web-browser of choice and browse http://localhost:8080/api/Test.
9. Go to *console2*. mdbg.exe will have stopped at the breakpoint displaying the following:

	```
	STOP UserBreak
	located at line 17 in
	[p#:0, t#:8] mdbg>
	```

When using only one file, the correct line number is displayed (or almost). This could get complicated if you have multiple files and various breakpoints. That would be another of the uses for `#debug`, which could be replaced by the following so scriptcs could give you an idea of where you are standing:

```
Console.WriteLine("{0}:{1}", originalFileName, originalLineNumber);
Debugger.Break();
```

Now you can run any mdbg.exe command. For example, running `print this` outputs the following:

```
[p#:0, t#:8] mdbg> print this
this=Submission#0+TestController
		_disposed=False
		_request=System.Net.Http.HttpRequestMessage
		_modelState=System.Web.Http.ModelBinding.ModelStateDictionary
		_configuration=System.Web.Http.SelfHost.HttpSelfHostConfiguration
		_controllerContext=System.Web.Http.Controllers.HttpControllerContext
		_urlHelper=<null>
```

Inside the bin folder you will find server.pdb and server.dll.

If you have any feedback feel free to open an issue.