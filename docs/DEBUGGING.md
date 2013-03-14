scriptcs Debugging overview
===========================
This document explains by example the steps required to use Visual Studio to debug .csx files that are executed with scriptcs. Hopefully you won't need to debug very often, but if you are in need be sure to follow this example.

Prerequisites
--------------
1. The following example shows how you can debug the [WebApiHost sample](https://github.com/scriptcs/scriptcs-samples/tree/master/webapihost). This procedure assumes that you have the .csx, packages.config and Packages folder already setup.
2. You must have the [Roslyn CTP](http://msdn.microsoft.com/en-us/vstudio/roslyn.aspx) installed to get VS to recognize .csx files.

Steps
-----
1. Open Visual Studio.
2. Open the **Open Project** dialog by navigating to File -> Open -> Project/Solution.
	[openProject]
	The resulting solution explorer should look like this:
	[solutionExplorer]
3. Right-click the scriptcs solution item and click **Properties**.
4. Provide values for the following fields:
	* Arguments: `server.csx -debug`
	* Working directory: the source folder of the app you want to debug, in this the directory where server.csx is located.
5. Close the **Properties** window and save the solution.
6. Add server.csx to the solution by right-clicking the solution and selecting **Add Existing Item**.
7. Set a breakpoint in the `return "Hello World";` line of the **TestController**.
8. Press F5.
9. Open any browser and navigate to localhost:8080/api/test.

That's it, the breakpoint will be hit. You have all the goodness of VS, such as the Immediate Window, Add Watch to help you debugging.