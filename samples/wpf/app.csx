#load utilities.csx
#load mvvm.csx

#load viewmodels.csx
#load views.csx

using System;
using System.Windows;

public class App : Application 
{ 
    protected override void OnStartup(StartupEventArgs e)
	{
		var viewModel = new CalculatorViewModel();

		var window = new CalculatorView
		{
			DataContext = viewModel, 
			SizeToContent = SizeToContent.WidthAndHeight
		};

		window.Show();
	}
}

new App().Run();
