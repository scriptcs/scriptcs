# scriptcs

[![Chocolatey Version](http://img.shields.io/chocolatey/v/scriptcs.svg?style=flat-square)](http://chocolatey.org/packages/scriptcs) [![Chocolatey Downloads](http://img.shields.io/chocolatey/dt/scriptcs.svg?style=flat-square)](http://chocolatey.org/packages/scriptcs) [![NuGet version (ScriptCs.Hosting)](https://img.shields.io/nuget/v/ScriptCs.Hosting.svg?style=flat-square)](https://www.nuget.org/packages/ScriptCs.Hosting/)

[![*nix Build Status](http://img.shields.io/travis/scriptcs/scriptcs/dev.svg?style=flat-square&label=linux-build)](https://travis-ci.org/scriptcs/scriptcs) [![Windows Build Status](http://img.shields.io/teamcity/codebetter/Scriptcs_Ci.svg?style=flat-square&label=windows-build)](http://ci.scriptcs.net) [![Coverity Scan Build Status](https://img.shields.io/badge/coverity-passed-brightgreen.svg?style=flat-square)](https://scan.coverity.com/projects/2356)

[![Issue Stats](http://issuestats.com/github/scriptcs/scriptcs/badge/pr?style=flat-square)](http://issuestats.com/github/scriptcs/scriptcs) [![Issue Stats](http://issuestats.com/github/scriptcs/scriptcs/badge/issue?style=flat-square)](http://issuestats.com/github/scriptcs/scriptcs)

## What is it?

scriptcs makes it easy to write and execute C# with a simple text editor.

While Visual Studio, and other IDEs, are powerful tools, they can sometimes hinder productivity more than they promote it. You don’t always need, or want, the overhead of a creating a new solution or project. Sometimes you want to just type away in your favorite text editor.

scriptcs frees you from Visual Studio, without sacrificing the advantages of a strongly-typed language. 

* Write C# in your favorite text editor.
* Use NuGet to manage your dependencies.
* The relaxed C# scripting syntax means you can write and execute an application with only one line of code. 
* Script Packs allow you to bootstrap the environment for new scripts, further reduces the amount of code necessary to take advantage of your favorite C# frameworks.


## Getting scriptcs

Releases and nightly builds should be installed using [Chocolatey](http://chocolatey.org/). To install Chocolatey, execute the following command in your command prompt:

    @powershell -NoProfile -ExecutionPolicy Unrestricted -Command "iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%systemdrive%\chocolatey\bin

If the above fails with the error indicating that proxy authentication is required (i.e. [HTTP 407](http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.8)) then try again with the following on the command prompt that uses your default credentials:

    @powershell -NoProfile -ExecutionPolicy Unrestricted -Command "[Net.WebRequest]::DefaultWebProxy.Credentials = [Net.CredentialCache]::DefaultCredentials; iex ((New-Object Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%systemdrive%\chocolatey\bin

**Note:** If you are using a version of Chocolatey > 0.9.9.0 you can pass the `-y` into the install and upgrade commands to prevent the confirmation that will appear.

### Installing scriptcs

Once Chocolatey has been installed, you can install the latest stable version of scriptcs from your command prompt:

    choco install scriptcs

Chocolatey will install scriptcs to `%LOCALAPPDATA%\scriptcs\` and update your PATH accordingly.

**Note:** You may need to restart your command prompt after the installation completes.

### Staying up-to-date

With Chocolatey, keeping scriptcs updated is just as easy:

    choco upgrade scriptcs

**Note:** If you are using a version of Chocolatey < 0.9.0.0 you will need to use `choco update scriptcs`, but also think about updating Chocolatey itself.

### Nightly builds

Nightly builds are hosted on [MyGet](https://www.myget.org/), and can also be installed through with Chocolatey:

    choco install scriptcs -pre -source https://www.myget.org/F/scriptcsnightly/ 

### Building from source

#### Windows

1. Ensure you have .NET Framework 4.5 installed.

1. Execute the build script.

    `build.cmd`

#### Linux

1. Ensure you have Mono development tools 3.0 or later installed.

    `sudo apt-get install mono-devel`

1. Ensure your mono instance has root SSL certificates

    `mozroots --import --sync`
    
1. Execute the build script

    `./build.sh`

## Getting Started

### Using the REPL
The scriptcs REPL can be started by running scriptcs without any parameters. The REPL allows you to execute C# statements directly from your command prompt.

```batchfile
C:\> scriptcs
scriptcs (ctrl-c or blank to exit)

> var message = "Hello, world!";
> Console.WriteLine(message);
Hello, world!
> 

C:\>
```

REPL supports all C# language constructs (i.e. class definition, method definition), as well as multi-line input. For example:

```batchfile
C:\> scriptcs
scriptcs (ctrl-c or blank to exit)

> public class Test {
    public string Name { get; set; }
  }
> var x = new Test { Name = "Hello" };
> x
{Name: "Hello"}

C:\>
```

### Writing a script

* In an empty directory, create a new file named `app.csx`:

```c#
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

Console.WriteLine("Starting RavenDB server...");

EmbeddableDocumentStore documentStore = null;
try
{
    documentStore = new EmbeddableDocumentStore { UseEmbeddedHttpServer = true };
    documentStore.Initialize();

    var url = string.Format("http://localhost:{0}", documentStore.Configuration.Port);
    Console.WriteLine("RavenDB started, listening on {0}.", url);

    Console.ReadKey();
}
finally
{
    if (documentStore != null)
        documentStore.Dispose();
}
```

* Install the [RavenDB.Embedded](https://nuget.org/packages/RavenDB.Embedded/) package from NuGet using the [install command](https://github.com/scriptcs/scriptcs/wiki/Package-installation).

```batchfile
scriptcs -install RavenDB.Embedded
```

* Execute your script. Note that listening on a port requires that the command prompt be launched using the **Run as Administrator** option.

```batchfile
> scriptcs app.csx
INFO : Starting to create execution components
INFO : Starting execution
Starting RavenDB server...
.. snip ..
RavenDB started, listening on http://localhost:8080.
```

* Navigating to the URL that Raven is listening on will now bring up the RavenDB management studio.

### Bootstrap scripts with Script Packs

Script Packs can be used to further reduce the amount of code you need to write when working with common frameworks. 

* In an empty directory, install the [ScriptCs.WebApi](https://nuget.org/packages/ScriptCs.WebApi/) script pack from NuGet. The script pack automatically imports the Web API namespaces and provides a convenient factory method for initializing the Web API host. It also replaces the default `ControllerResolver` with a custom implementation that allows Web API to discover controllers declared in scripts.

```batchfile
scriptcs -install ScriptCs.WebApi
```

* Script packs can be imported into a script by calling `Require<TScriptPack>()`. Create a file named `server.csx` that contains the following code:

```c#
public class TestController : ApiController {
    public string Get() {
        return "Hello world!";
    }
}

var webApi = Require<WebApi>();
var server = webApi.CreateServer("http://localhost:8888");
server.OpenAsync().Wait();

Console.WriteLine("Listening...");
Console.ReadKey();
server.CloseAsync().Wait();
```

* In a command prompt running as administrator, execute the `server.csx` file.

```batchfile
scriptcs server.csx 
```

* Browse to http://localhost:8888/test/ to see the result of the TestController.Get method.

```xml
<string xmlns="http://schemas.microsoft.com/2003/10/Serialization/">Hello world!</string>
```

### Referencing scripts

* Move the TestController class from the previous example into a new file named `controller.csx` with the following content.

* On the first line of `server.csx`, reference `controller.csx` using the [#load directive](https://github.com/scriptcs/scriptcs/wiki/Writing-a-script#loading-referenced-scripts). **Note:** #load directives must be placed at the top of a script, otherwise they will be ignored.

```c#
#load "controller.csx"
```

* In a command prompt running as administrator, execute the `server.csx` file.

```batchfile
scriptcs server.csx 
```

* Browse to http://localhost:8888/test/ to see the result of the TestController.Get method.

```xml
<string xmlns="http://schemas.microsoft.com/2003/10/Serialization/">Hello world!</string>
```


### Referencing assemblies

You can reference additional assemblies from the GAC or from the `bin` folder in your script's directory using the [#r directive](https://github.com/scriptcs/scriptcs/wiki/Writing-a-script#referencing-assemblies):

```c#
#r "nunit.core.dll"
#r "nunit.core.interfaces.dll"

var path = "UnitTests.dll";
var runner = TestSetup.GetRunner(new[] {path});
var result = runner.Run(new ConsoleListener(msg => Console.WriteLine(msg)), TestFilter.Empty, true,     LoggingThreshold.All);

Console.ReadKey();
```

### Debugging

Instructions for debugging scripts using Visual Studio can be found on the [wiki](https://github.com/scriptcs/scriptcs/wiki/Debugging-a-script).

### Package installation

You can install any NuGet packages directly from the scriptcs CLI. This will pull the relevant packages from NuGet, and install them in the scriptcs_packages folder.

Once the packages are installed, you can simply start using them in your script code directly (just import the namespaces - no additional bootstrapping or DLL referencing is needed).

The `install` command will also create a `scriptcs_packages.config` file if you don't have one - so that you can easily redistribute your script (without having to copy the package binaries).

 - `scriptcs -install {package name}` will install the desired package from NuGet. 
 	
	For example: 

		scriptcs -install ServiceStack
		
 - `scriptcs -install` (without package name) will look for the `scriptcs_packages.config` file located in the current execution directory, and install all the packages specified there. You only need to specify **top level** packages.

For example, you might create the following `scriptcs_packages.config`:

	<?xml version="1.0" encoding="utf-8"?>
	<packages>
  		<package id="Nancy.Hosting.Self" version="0.16.1" targetFramework="net40" />
  		<package id="Nancy.Bootstrappers.Autofac" version="0.16.1" targetFramework="net40" />
  		<package id="Autofac" version="2.6.3.862" targetFramework="net40" />
	</packages>

And then just call:

    scriptcs -install

As a result, all packages specified in the `scriptcs_packages.config`, including all dependencies, will be downloaded and installed in the `scriptcs_packages` folder. 


## Contributing

* Read our [Contribution Guidelines](https://github.com/scriptcs/scriptcs/blob/master/CONTRIBUTING.md). 


## Samples and Documentation

Additional samples can be contributed to our [samples repository](https://github.com/scriptcs/scriptcs-samples). Documentation can be found on our [wiki](https://github.com/scriptcs/scriptcs/wiki). 


## Community

Want to chat? In addition to Twitter, you can find us on [Google Groups](https://groups.google.com/forum/?fromgroups#!forum/scriptcs) and [JabbR](https://jabbr.net/#/rooms/scriptcs)!


## Coordinators

* [Glenn Block](https://github.com/glennblock) ([@gblock](https://twitter.com/intent/user?screen_name=gblock))
* [Justin Rusbatch](https://github.com/jrusbatch) ([@jrusbatch](https://twitter.com/intent/user?screen_name=jrusbatch))
* [Filip Wojcieszyn](https://github.com/filipw) ([@filip_woj](https://twitter.com/intent/user?screen_name=filip_woj))


## Core Committers

* [Damian Schenkelman](http://github.com/dschenkelman) ([@dschenkelman](https://twitter.com/intent/user?screen_name=dschenkelman))
* [Kristian Hellang](http://github.com/khellang) ([@khellang](https://twitter.com/intent/user?screen_name=khellang))
* [Adam Ralph](http://github.com/adamralph) ([@adamralph](https://twitter.com/intent/user?screen_name=adamralph))
* [Paul Bouwer](http://github.com/paulbouwer) ([@pbouwer](https://twitter.com/intent/user?screen_name=pbouwer))

## Credits 

* Check out the [list of developers](https://github.com/scriptcs/scriptcs/wiki/Contributors) responsible for getting scriptcs to where it is today! 
* Special thanks to Filip Wojcieszyn for being the inspiration behind this with his Roslyn Web API posts.
* Thanks to the Roslyn team who helped point me in the right direction.


## License 

[Apache 2 License](https://github.com/scriptcs/scriptcs/blob/master/LICENSE.md)
