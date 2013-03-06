#load "usings.csx"
#load "models.csx"
#load "broadcaster.csx"
#load "jsonconfiguration.csx"

const string ConfigFileName = "config.json";

if (!JsonConfiguration.Exists(ConfigFileName))
{
    var defaultInstance = new LiveReloadConfig()
    {
        Server = "http://localhost:8080/",
        Path = ".",
        Extensions = new []{ ".cshtml", ".css", ".js", ".html" }
    };
    
    JsonConfiguration.Put(ConfigFileName, defaultInstance);

    Console.WriteLine("Missing config.json. One has been created for your convenience.");
    Console.WriteLine("No thank you necessary...");
    Environment.Exit(0);
}

Console.WriteLine("Loading config.json");

var config = JsonConfiguration.Get<LiveReloadConfig>(ConfigFileName);

Console.WriteLine("About to connect to SignalR running at {0}...", config.Server);
Console.WriteLine("About to start watching {0} for changes to {1}", config.Path, string.Join(",", config.Extensions));

var broadcaster = new LiveReloadBroadcaster(config.Server, config.Path, config.Extensions);
broadcaster.Start();

Console.WriteLine("Press any key to continue...");
Console.ReadKey(true);

broadcaster.Stop();
