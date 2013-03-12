# ScriptCS

## Why should you care?
Write C# applications with a text editor, NuGet and the power of Roslyn!

**Note**: *Roslyn is a pre-release community technology preview (CTP) and currently an unsupported technology. As such there may be changes in Roslyn itself that could impact this project. Please bear that in mind when using ScriptCS.*

* More on why I developed this [here] (http://codebetter.com/glennblock/2013/02/28/scriptcs-living-on-the-edge-in-c-without-a-project-on-the-wings-of-roslyn-and-nuget/)
* Check out our goals and roadmap [here] (https://github.com/scriptcs/scriptcs/wiki/Project-goals-and-roadmap)

## Pre-requirements
* Install the [NuGet cmdline bootstrapper] (http://nuget.codeplex.com/releases/view/58939)
* Build the project and put scriptcs.exe in your path.

## Quick start
* Open a cmd prompt as administrator
* Create a directory "c:\scriptcs_hello" and change to it.
* Run "nuget install Microsoft.AspNet.WebApi.SelfHost -o Packages"
* Create a server.csx with your favorite editor. Paste the text below into the file and save.

```csharp
using System;
using System.IO;
using System.Web.Http;
using System.Web.Http.SelfHost;

var address = "http://localhost:8080";
var conf = new HttpSelfHostConfiguration(new Uri(address));
conf.Routes.MapHttpRoute(name: "DefaultApi",
   routeTemplate: "api/{controller}/{id}",
   defaults: new { id = RouteParameter.Optional }
);

var server = new HttpSelfHostServer(conf);
server.OpenAsync().Wait();
Console.WriteLine("Listening...");
Console.ReadKey();
```
* run "scriptcs server.csx"

This will launch a web API host.

## How it works
ScriptCS relies on Roslyn for loading loose C# script files. It will automatically discover NuGet packages local to the application and load the binaries.

## What's next
* Adding support for pluggable recipe "packs" for different frameworks.

## Contributing
Read our [Contribution Guidelines](https://github.com/scriptcs/scriptcs/blob/master/CONTRIBUTING.md). 

## Credits
* Special thanks to [@filip_woj](http://twitter.com/filip_woj) for being the inspiration behind this with his Roslyn Web API posts.
* Thanks to the Roslyn team who helped pointing me in the right direction.

## Coordinators
* Glenn Block ([@gblock](https://twitter.com/gblock))
* Justin Rusbatch ([@jrusbatch](https://twitter.com/jrusbatch))
* Filip Wojcieszyn ([@filip_woj](https://twitter.com/filip_woj))

## License 
[Apache 2 License](https://github.com/scriptcs/scriptcs/blob/master/LICENSE.md)

