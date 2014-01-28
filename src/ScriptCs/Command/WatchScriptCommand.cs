using System;
using System.Threading;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class WatchScriptCommand : IScriptCommand
    {
        private readonly AppDomainSetup _setup = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
        private readonly ScriptCsArgs _commandArgs;
        private readonly string[] _scriptArgs;
        private readonly IConsole _console;
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;

        public WatchScriptCommand(ScriptCsArgs commandArgs, string[] scriptArgs, IConsole console, IFileSystem fileSystem, ILog logger)
        {
            Guard.AgainstNullArgument("commandArgs", commandArgs);
            Guard.AgainstNullArgument("console", console);
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("logger", logger);

            _commandArgs = commandArgs;
            _scriptArgs = scriptArgs;
            _console = console;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public CommandResult Execute()
        {
            var command = new CrossAppDomainExecuteScriptCommand
            {
                CommandArgs = _commandArgs,
                ScriptArgs = _scriptArgs,
            };

            using (var @event = new ManualResetEventSlim())
            using (var watcher = new FileWatcher(_commandArgs.ScriptName, 500, _fileSystem))
            {
                watcher.Changed += (sender, e) => @event.Set();

                _logger.DebugFormat("Starting file watcher for '{0}'", _commandArgs.ScriptName);
                watcher.Start();

                while (true)
                {
                    _logger.DebugFormat("Creating app domain '{0}'", _commandArgs.ScriptName);
                    var appDomain = AppDomain.CreateDomain(_commandArgs.ScriptName, null, _setup);
                    try
                    {
                        _logger.DebugFormat("Executing script in app domain '{0}'", _commandArgs.ScriptName);
                        @event.Reset();
                        appDomain.DoCallBack(command.Execute);
                    }
                    finally
                    {
                        EnsureUnloaded(appDomain);
                    }

                    _logger.InfoFormat("Watching '{0}' for changes... (ctrl-c to exit)", _commandArgs.ScriptName);
                    @event.Wait();
                    _logger.InfoFormat("'{0}' changed. Reloading...", _commandArgs.ScriptName);
                }
            }
        }

        private void EnsureUnloaded(AppDomain domain)
        {
            if (domain.IsFinalizingForUnload())
            {
                _logger.DebugFormat("App domain '{0}' is already unloading", domain.FriendlyName);
                return;
            }

            try
            {
                _logger.DebugFormat("Unloading app domain '{0}'", domain.FriendlyName);
                AppDomain.Unload(domain);
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Error unloading app domain '{0}'", ex, domain.FriendlyName);
            }
        }

        public string[] ScriptArgs
        {
            get { return this._scriptArgs; }
        }
    }
}
