using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

public static class Utilities
{
	public static void LoadXaml(ContentControl contentControl, string xamlFile)
	{
		using (var fileStream = File.OpenRead(xamlFile))
		{
			contentControl.Content = XamlReader.Load(fileStream) as DependencyObject;
		}
	}
	
	public static void RunInSTAThread(ThreadStart threadStart)
	{
		var thread = new Thread(threadStart);
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
	}
}