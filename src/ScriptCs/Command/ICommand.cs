namespace ScriptCs.Command
{
    public interface IScriptCommand : ICommand { }

    public interface IRestoreCommand : ICommand { }

    public interface IInstallCommand : ICommand { }

    public interface IInvalidCommand : ICommand { }

    public interface ICommand
    {
        int Execute();
    }
}