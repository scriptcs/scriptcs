#load "models.csx"
#load "service.csx"

using System;
using ServiceStack.WebHost.Endpoints;
using System.Reflection;

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