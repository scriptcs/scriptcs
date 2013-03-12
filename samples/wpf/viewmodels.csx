using System;
using System.Windows.Input;

public class CalculatorViewModel : ViewModelBase
{
	private string _display;

	public CalculatorViewModel()
	{
		Display = "0";
		
		NumberCommand = new RelayCommand(param => AddNumber(Convert.ToInt32(param)), param => CanAddNumber);

		AddCommand = CreateOperatorCommand('+');
		SubtractCommand = CreateOperatorCommand('-');
		MultiplyCommand = CreateOperatorCommand('*');
		DivideCommand = CreateOperatorCommand('/');

		CalculateCommand = new RelayCommand(param => Calculate(), param => CanCalculate);
		ClearCommand = new RelayCommand(param => Clear(), param => true);
	}

	public string Display
	{
		get { return _display; }
		set
		{
			_display = value;
			NotifyPropertyChanged("Display");
			CommandManager.InvalidateRequerySuggested();
		}
	}

	public RelayCommand NumberCommand { get; private set; }

	public RelayCommand AddCommand { get; private set; }

	public RelayCommand SubtractCommand { get; private set; }

	public RelayCommand MultiplyCommand { get; private set; }

	public RelayCommand DivideCommand { get; private set; }

	public RelayCommand CalculateCommand { get; private set; }

	public RelayCommand ClearCommand { get; private set; }

	private int? Operand1 { get; set; }

	private char? Operator { get; set; }

	private int? Operand2 { get; set; }

	private int? Result { get; set; }

	private bool CanApplyOperator
	{
		get { return Operand1.HasValue && !Operator.HasValue; }
	}

	private void ApplyOperator(char @operator)
	{
		Operator = @operator;
		Display = string.Format("{0} {1} ", Display, Operator);
	}

	private bool CanAddNumber
	{
		get { return !Result.HasValue; }
	}

	private void AddNumber(int number)
	{
		if (Operator.HasValue)
		{
			Operand2 = !Operand2.HasValue ? number : (Operand2 * 10) + number;
			Display += number;
			return;
		}

		Operand1 = !Operand1.HasValue ? number : (Operand1 * 10) + number;
		Display += number;
	}

	private bool CanCalculate
	{
		get { return Operand1.HasValue && Operator.HasValue && Operand2.HasValue && !Result.HasValue; }
	}

	private void Calculate()
	{
		switch (Operator)
		{
			case '+':
				Result = Operand1 + Operand2;
				break;
			case '-':
				Result = Operand1 - Operand2;
				break;
			case '*':
				Result = Operand1 * Operand2;
				break;
			case '/':
				Result = Operand1 / Operand2;
				break;
		}

		Display = Result.ToString();
	}

	private void Clear()
	{
		Operand1 = null;
		Operator = null;
		Operand2 = null;
		Result = null;
		Display = "0";
	}

	private RelayCommand CreateOperatorCommand(char @operator)
	{
		return new RelayCommand(param => ApplyOperator(@operator), param => CanApplyOperator);
	}
}