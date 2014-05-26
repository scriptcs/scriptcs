using System;

namespace ScriptCs.Command
{
    internal class DeferredCreationCommand<TCommand> : IDeferredCreationCommand<TCommand> where TCommand : ICommand
    {
        private readonly Func<TCommand> _factory;

        public DeferredCreationCommand(Func<TCommand> factory)
        {
            Guard.AgainstNullArgument("factory", factory);

            _factory = factory;
        }

        public CommandResult Execute()
        {
            return _factory().Execute();
        }
    }
}