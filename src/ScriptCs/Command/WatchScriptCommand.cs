using System;
using System.Threading;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class WatchScriptCommand : IScriptCommand
    {
        private readonly AppDomainSetup _setup = new AppDomainSetup
        {
            ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
        };

        private readonly Config _config;
        private readonly string[] _scriptArgs;
        private readonly IConsole _console;
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;
        private readonly IFileSystemMigrator _fileSystemMigrator;
        private readonly CrossAppDomainExecuteScriptCommand _executeScriptCommand;

        public WatchScriptCommand(
            Config config,
            string[] scriptArgs,
            IConsole console,
            IFileSystem fileSystem,
            ILogProvider logProvider,
            IFileSystemMigrator fileSystemMigrator)
        {
            Guard.AgainstNullArgument("config", config);
            Guard.AgainstNullArgument("scriptArgs", scriptArgs);
            Guard.AgainstNullArgument("console", console);
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("logProvider", logProvider);
            Guard.AgainstNullArgument("fileSystemMigrator", fileSystemMigrator);

            _config = config;
            _scriptArgs = scriptArgs;
            _console = console;
            _fileSystem = fileSystem;
            _logger = logProvider.ForCurrentType();
            _fileSystemMigrator = fileSystemMigrator;

            _executeScriptCommand = new CrossAppDomainExecuteScriptCommand
            {
                Config = _config,
                ScriptArgs = _scriptArgs,
            };
        }

        public CommandResult Execute()
        {
            _fileSystemMigrator.Migrate();

            _console.WriteLine("scriptcs (ctrl-c to exit)");
            _logger.InfoFormat("Running script '{0}' and watching for changes...", _config.ScriptName);

            while (true)
            {
                using (var fileChanged = new ManualResetEventSlim())
                using (var watcher = new FileWatcher(_config.ScriptName, 500, _fileSystem))
                {
                    _logger.DebugFormat("Creating app domain '{0}'...", _config.ScriptName);
                    var appDomain = AppDomain.CreateDomain(_config.ScriptName, null, _setup);
                    try
                    {
                        watcher.Changed += (sender, e) =>
                        {
                            _logger.DebugFormat("Script '{0}' changed.", _config.ScriptName);
                            EnsureUnloaded(appDomain);
                            fileChanged.Set();
                        };

                        watcher.Start();
                        _logger.DebugFormat("Executing script '{0}' and watching for changes...", _config.ScriptName);
                        fileChanged.Reset();
                        try
                        {
                            appDomain.DoCallBack(_executeScriptCommand.Execute);
                        }
                        catch (AppDomainUnloadedException ex)
                        {
                            _logger.DebugFormat("App domain '{0}' has been unloaded.", ex, _config.ScriptName);
                        }
                    }
                    finally
                    {
                        EnsureUnloaded(appDomain);
                    }

                    fileChanged.Wait();
                    _logger.InfoFormat("Script changed. Reloading...", _config.ScriptName);
                }
            }
        }

        private void EnsureUnloaded(AppDomain domain)
        {
            try
            {
                _logger.DebugFormat("Unloading app domain '{0}'", _config.ScriptName);
                AppDomain.Unload(domain);
            }
            catch (AppDomainUnloadedException ex)
            {
                _logger.DebugFormat("App domain '{0}' has already been unloaded.", ex, _config.ScriptName);
            }
        }

        public string[] ScriptArgs
        {
            get { return this._scriptArgs; }
        }
    }
}
