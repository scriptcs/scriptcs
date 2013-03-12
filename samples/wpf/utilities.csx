using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

public static class XamlUtility
{
	public static void LoadXaml(ContentControl contentControl, string xamlFile)
	{
		using (var fileStream = File.OpenRead(xamlFile))
		{
			contentControl.Content = XamlReader.Load(fileStream) as DependencyObject;
		}
	}
}