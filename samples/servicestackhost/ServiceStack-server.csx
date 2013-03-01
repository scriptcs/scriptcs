using System;
using ServiceStack.ServiceInterface;
using ServiceStack.WebHost.Endpoints;
using System.Reflection;

public class Hello {
	public string Name { get; set; }
}

public class HelloResponse {
	public string Result { get; set; }
}

public class HelloService : Service
{
	public object Any(Hello request) 
	{
		return new HelloResponse { Result = "Hello, " + request.Name };
	}
}

public class AppHost : AppHostHttpListenerBase {
	public AppHost() : base("StarterTemplate HttpListener", Assembly.GetExecutingAssembly()) { }

	public override void Configure(Funq.Container container) {
		Routes
			.Add<Hello>("/hello")
			.Add<Hello>("/hello/{Name}");
	}
}

var port = "http://*:999/";
var appHost = new AppHost();
appHost.Init();
appHost.Start(port);

Console.WriteLine("listening on {0}", port);
Console.ReadKey();