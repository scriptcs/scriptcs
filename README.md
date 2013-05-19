# scriptcs


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

### Installing scriptcs

Once Chocolatey has been installed, you can install the latest stable version of scriptcs from your command prompt:

    cinst scriptcs

Chocolatey will install scriptcs to `%APPDATA%\scriptcs\` and update your PATH accordingly.

**Note:** You may need to restart your command prompt after the installation completes.

### Staying up-to-date

With Chocolatey, keeping scriptcs updated is just as easy:

    cup scriptcs

### Nightly builds

Nightly builds are hosted on [MyGet](https://www.myget.org/), and can also be installed through with Chocolatey:

    cinst scriptcs -pre -source https://www.myget.org/F/scriptcsnightly/ 

### Building from source

Execute `build.cmd` to start the build script.


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

* In an empty directory, install the [ScriptCs.WebApi](https://nuget.org/packages/ScriptCs.WebApi/) script pack from NuGet. The script pack will automatically imports the Web API namespaces and provides a convenient factory method for initializing the Web API host. It also replaces the default `ControllerResolver` with a custom implementation that allows Web API to discover controllers declared in scripts.

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

You can reference additional assemblies from the GAC or from the bin folder in your script's directory using the [#r directive](https://github.com/scriptcs/scriptcs/wiki/Writing-a-script#referencing-assemblies):

```c#
#r "nunit.core.dll"
#r "nunit.core.interfaces.dll"

var path = "UnitTests.dll";
var runner = TestSetup.GetRunner(new[] {path});
var result = runner.Run(new ConsoleListener(msg => Console.WriteLine(msg)), TestFilter.Empty, true,     LoggingThreshold.All);

Console.ReadKey();
```

### Passing arguments to the script

You can pass any additional arguments to the script by using `-args` command-line switch:

```batchfile
scriptcs deploy.csx -args "Dev -label 'Next Week RC' -version 1110 -force"
```

Argument parsing is done by using excellent [PowerArgs](https://github.com/adamabdelhamed/PowerArgs) library, so you just need to declare class, similar to the one below (see [PowerArgs](https://github.com/adamabdelhamed/PowerArgs) documentation), to get your arguments inside the script:

```c#
public class DeployArgs
{
	[ArgPosition(0)]
	[ArgDescription("Environment code")]
	public string Environment { get; set; }

	[ArgShortcut("label")]
	[ArgDescription("Deployment label")]
	public string Label { get; set; }

	[ArgShortcut("version")]
	[ArgDescription("Version to deploy")]
	public int Version { get; set; }

	[ArgShortcut("force")]
	[ArgDescription("Forces re-deployment")]
	public bool Force { get; set; }
}

var args = Args<DeployArgs>();
Console.WriteLine(args.Environment);
Console.WriteLine(args.Label);
Console.WriteLine(args.Version);
Console.WriteLine(args.Force);
```
You can also get raw command-line input if you'd like to parse script arguments yourself:

```c#
var args = Args();
Console.WriteLine(args);
```

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


## Credits 

* Check out the [list of developers](https://github.com/scriptcs/scriptcs/wiki/Contributors) responsible for getting scriptcs to where it is today! 
* Special thanks to Filip Wojcieszyn for being the inspiration behind this with his Roslyn Web API posts.
* Thanks to the Roslyn team who helped point me in the right direction.


## License 

[Apache 2 License](https://github.com/scriptcs/scriptcs/blob/master/LICENSE.md)
