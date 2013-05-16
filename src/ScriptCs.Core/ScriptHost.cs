using System;
using System.Linq;
using System.Text.RegularExpressions;

using PowerArgs;

using ScriptCs.Contracts;
using ScriptCs.Exceptions;

namespace ScriptCs
{
    public class ScriptHost
    {
	    readonly string _scriptArgs;
	    readonly IScriptPackManager _scriptPackManager;

	    object _parsedArgs;

        public ScriptHost(string scriptArgs, IScriptPackManager scriptPackManager)
        {
	        _scriptArgs = scriptArgs ?? "";
	        _scriptPackManager = scriptPackManager;
        }

	    public T Require<T>() where T:IScriptPackContext
        {
            return _scriptPackManager.Get<T>();
        }

	    public string Args()
	    {
		    return _scriptArgs;
	    }

		public TArgs Args<TArgs>() where TArgs:class, new()
		{
			if (_parsedArgs == null)
				_parsedArgs = Args() != "" ? TryParseArgs<TArgs>() : new TArgs();

			return (TArgs) _parsedArgs;
		}

	    TArgs TryParseArgs<TArgs>() where TArgs:class, new()
	    {
		    try
		    {
			    return PowerArgs.Args.Parse<TArgs>(SplitArgs());
		    }
		    catch (ArgException)
		    {
				var options = new ArgUsageOptions { ShowPosition = false, ShowType = false };
				var usage = ArgUsage.GetUsage<TArgs>(options: options);

			    throw new ScriptExecutionException("Wrong script arguments. See usage below\n\n" + usage);
		    }
	    }

	    string[] SplitArgs()
	    {
			var regex = new Regex(@"(--?[\w]+)[:\s=]?([\w]:(?:\\[\w\s-]+)+\\?(?=\s-)|'[^']*'|[^-][^\s]*)?");

		    return regex.Split(Args())
					.Where(arg => !string.IsNullOrWhiteSpace(arg))
					.Select(arg => arg.TrimEnd(' ').Trim('\''))
					.ToArray();
	    }
    }
}
