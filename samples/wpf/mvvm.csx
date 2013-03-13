using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

public class ViewModelBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void NotifyPropertyChanged(string propertyName)
	{
		var handler = PropertyChanged;
		if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class RelayCommand : ICommand
{
	private readonly Action<object> _execute;

	private readonly Func<object, bool> _canExecute;

	public RelayCommand(Action<object> execute) : this(execute, null) { }

	public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
	{
		if (execute == null) throw new ArgumentNullException("execute");

		_execute = execute;
		_canExecute = canExecute;
	}

	[DebuggerStepThrough]
	public bool CanExecute(object parameter)
	{
		return _canExecute == null || _canExecute(parameter);
	}

	public event EventHandler CanExecuteChanged
	{
		add { CommandManager.RequerySuggested += value; }
		remove { CommandManager.RequerySuggested -= value; }
	}

	public void Execute(object parameter)
	{
		_execute(parameter);
	}
}