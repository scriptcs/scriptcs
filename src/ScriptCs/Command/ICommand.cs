using System.Collections.Generic;

namespace ScriptCs.Command
{
    public interface IScriptCommand : ICommand { }

    public interface IRestoreCommand : ICommand { }

    public interface ISaveCommand : ICommand { }

    public interface ICleanCommand : ICommand { }

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