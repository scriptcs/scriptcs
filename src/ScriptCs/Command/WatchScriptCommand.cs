﻿using System;
using System.Threading;
using Common.Logging;
using ScriptCs.Contracts;

namespace ScriptCs.Command
{
    internal class WatchScriptCommand : ICommand
    {
        private readonly AppDomainSetup _setup = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
        private readonly ScriptCsArgs _commandArgs;
        private readonly string[] _scriptArgs;
        private readonly IConsole _console;
        private readonly IFileSystem _fileSystem;
        private readonly ILog _logger;
        private readonly CrossAppDomainExecuteScriptCommand _executeScriptCommand;

        public WatchScriptCommand(ScriptCsArgs commandArgs, string[] scriptArgs, IConsole console, IFileSystem fileSystem, ILog logger)
        {
            Guard.AgainstNullArgument("commandArgs", commandArgs);
            Guard.AgainstNullArgument("scriptArgs", scriptArgs);
            Guard.AgainstNullArgument("console", console);
            Guard.AgainstNullArgument("fileSystem", fileSystem);
            Guard.AgainstNullArgument("logger", logger);

            _commandArgs = commandArgs;
            _scriptArgs = scriptArgs;
            _console = console;
            _fileSystem = fileSystem;
            _logger = logger;
            _executeScriptCommand = new CrossAppDomainExecuteScriptCommand(commandArgs, scriptArgs);
        }

        public CommandResult Execute()
        {
            _console.WriteLine("scriptcs (ctrl-c to exit)");
            _logger.InfoFormat("Running script '{0}' and watching for changes...", _commandArgs.ScriptName);

            while (true)
            {
                using (var fileChanged = new ManualResetEventSlim())
                using (var watcher = new FileWatcher(_commandArgs.ScriptName, 500, _fileSystem))
                {
                    _logger.DebugFormat("Creating app domain '{0}'...", _commandArgs.ScriptName);
                    var appDomain = AppDomain.CreateDomain(_commandArgs.ScriptName, null, _setup);
                    try
                    {
                        watcher.Changed += (sender, e) =>
                        {
                            _logger.DebugFormat("Script '{0}' changed", _commandArgs.ScriptName);
                            EnsureUnloaded(appDomain);
                            fileChanged.Set();
                        };

                        watcher.Start();
                        _logger.DebugFormat("Executing script '{0}' and watching for changes...", _commandArgs.ScriptName);
                        fileChanged.Reset();
                        try
                        {
                            appDomain.DoCallBack(_executeScriptCommand.Execute);
                        }
                        catch (AppDomainUnloadedException)
                        {
                            _logger.DebugFormat("App domain '{0}' has been unloaded", _commandArgs.ScriptName);
                        }
                    }
                    finally
                    {
                        EnsureUnloaded(appDomain);
                    }

                    fileChanged.Wait();
                    _logger.InfoFormat("Script changed. Reloading...", _commandArgs.ScriptName);
                }
            }
        }

        private void EnsureUnloaded(AppDomain domain)
        {
            try
            {
                _logger.DebugFormat("Unloading app domain '{0}'", _commandArgs.ScriptName);
                AppDomain.Unload(domain);
            }
            catch (AppDomainUnloadedException)
            {
                _logger.DebugFormat("App domain '{0}' has already been unloaded.", _commandArgs.ScriptName);
            }
        }

        public string[] ScriptArgs
        {
            get { return this._scriptArgs; }
        }
    }
}
