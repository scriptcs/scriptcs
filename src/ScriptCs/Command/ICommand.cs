using System.Collections.Generic;

namespace ScriptCs.Command
{
    public interface IScriptCommand : ICommand {
        string[] ScriptArgs { get; }
    }

    public interface IRestoreCommand : ICommand { }

    public interface ISaveCommand : ICommand { }

    public interface ICleanCommand : ICommand { }

    public interface IInstallCommand : ICommand { }

    public interface IInvalidCommand : ICommand { }

    public interface IHelpCommand : ICommand { }

    public interface ICompositeCommand : ICommand
    {
        List<ICommand> Commands { get; }
    }

    public interface IVersionCommand : ICommand
    {
    }

    public interface ICommand
    {
        CommandResult Execute();
    }
}