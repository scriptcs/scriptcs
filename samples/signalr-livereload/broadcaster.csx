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
