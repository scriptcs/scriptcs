using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json;

public class LiveReloadConfig
{
    public string Server { get; set; }
    public string Path { get; set ; }
    public string[] Extensions { get; set; }
}

if (!File.Exists("config.json"))
{
    var defaultInstance = new LiveReloadConfig()
    {
        Server = "http://localhost:8080/",
        Path = ".",
        Extensions = new []{ ".cshtml", ".css", ".js", ".html" }
    };
    
    var defaultFile = JsonConvert.SerializeObject(defaultInstance, Formatting.Indented);
    File.WriteAllText("config.json", defaultFile);
    Console.WriteLine("Missing config.json. One has been created for your convenience.");
    Console.WriteLine("No thank you necessary...");
    Environment.Exit(0);
}

Console.WriteLine("Loading config.json");

var config = JsonConvert.DeserializeObject<LiveReloadConfig>(File.ReadAllText("config.json"));

Console.WriteLine("About to connect to SignalR running at {0}...", config.Server);
Console.WriteLine("About to start watching {0} for changes to {1}", config.Path, string.Join(",", config.Extensions));

public class LiveReloadBroadcaster
{
    private bool _initialized;
    private string _path;
    private string[] _extensions;
    private string[] _fileExtensions = { ".cshtml", ".js", ".css", ".html" };
    private FileSystemWatcher _fileSystemWatcher;
    private HubConnection _connection;
    private IHubProxy _liveReloadHub;

    public LiveReloadBroadcaster(string server, string path, string[] extensions)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException("path");
        }

        _path = Path.GetFullPath(path);

        if (!Directory.Exists(_path))
        {
            throw new DirectoryNotFoundException("Could not find " + _path);
        }

        _fileExtensions = extensions;
        _fileSystemWatcher = new FileSystemWatcher(_path);
        _fileSystemWatcher.IncludeSubdirectories = true;
        _fileSystemWatcher.Changed += FileSystemChanged;

        _connection = new HubConnection(server);
        _connection.Closed += ConnectionClosed;
        _connection.Error += ConnectionError;
        _connection.Reconnected += ConnectionReconnected;
        _liveReloadHub = _connection.CreateHubProxy("liveReload");
    }

    public void ConnectionClosed()
    {
        Console.WriteLine("Connection closed.");
    }

    public void ConnectionError(Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

    public void ConnectionReconnected()
    {
        Console.WriteLine("Reconnected.");
    }

    public void Start()
    {
        _fileSystemWatcher.EnableRaisingEvents = true;

        _connection.Start().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Console.WriteLine("There was an error opening the connection:{0}",
                                  task.Exception.GetBaseException());
            }
            else
            {
                Console.WriteLine("Connected");
            }

        }).Wait();
    }

    public void Stop()
    {
        _fileSystemWatcher.EnableRaisingEvents = false;
    }

    public void FileSystemChanged(object sender, FileSystemEventArgs e)
    {
        if (!_fileExtensions.Contains(Path.GetExtension(e.FullPath)))
        {
            return;
        }

        _liveReloadHub.Invoke<string>("ReloadAllClients").ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Console.WriteLine("There was an error calling send: {0}",
                                    task.Exception.GetBaseException());
            }
            else
            {
                Console.WriteLine(task.Result);
            }
        });
    }
}

var broadcaster = new LiveReloadBroadcaster(config.Server, config.Path, config.Extensions);
broadcaster.Start();

Console.WriteLine("Press any key to continue...");
Console.ReadKey(true);

broadcaster.Stop();

Console.WriteLine("Have a nice day.");
