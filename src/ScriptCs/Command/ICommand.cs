using System.Collections.Generic;

namespace ScriptCs.Command
{
    public interface IScriptCommand : ICommand { }

    public interface IRestoreCommand : ICommand { }

    public interface IInstallCommand : ICommand { }

    public interface IInvalidCommand : ICommand { }

    public interface ICompositeCommand : ICommand
    {
        List<ICommand> Commands { get; }
    }

    public interface ICommand
    {
        CommandResult Execute();
    }
}