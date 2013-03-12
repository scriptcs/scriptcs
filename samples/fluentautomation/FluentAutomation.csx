using FluentAutomation;
using FluentAutomation.Interfaces;
using System;
using System.IO;
using System.Reflection;

private static INativeActionSyntaxProvider I = null;

public static void Bootstrap<T>(string browserName)
{
    MethodInfo bootstrapMethod = null;
    ParameterInfo[] bootstrapParams = null;

    MethodInfo[] methods = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public);
    foreach (var methodInfo in methods)
    {
        if (methodInfo.Name.Equals("Bootstrap"))
        {
            bootstrapMethod = methodInfo;
            bootstrapParams = methodInfo.GetParameters();
            if (bootstrapParams.Length == 1)
            {
                break;
            }
        }
    }

    var browserEnumValue = Enum.Parse(bootstrapParams[0].ParameterType, browserName);
    bootstrapMethod.Invoke(null, new object[] { browserEnumValue });
	
	I = new FluentTest().I;

	// hack to move drivers into bin so they can be located by Selenium (only prob on scriptcs atm)
	foreach (var driver in Directory.GetFiles(Environment.CurrentDirectory, "*.exe"))
	{
		var newFileName = Path.Combine(Environment.CurrentDirectory, "bin", Path.GetFileName(driver));
		if (!File.Exists(newFileName)) File.Move(driver, newFileName);
	}
}