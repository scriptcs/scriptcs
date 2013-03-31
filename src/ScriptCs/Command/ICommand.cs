using System.Collections.Generic;

namespace ScriptCs.Command
{
    public interface IScriptCommand : ICommand { }

    public interface IRestoreCommand : ICommand { }

    public interface ICleanCommand : ICommand { }

    public interface IInstallCommand : ICommand { }

    public interface IInvalidCommand : ICommand { }

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