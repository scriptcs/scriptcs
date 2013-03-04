#load "utility.csx"

using System;

var time = PerformanceUtilities.ExecutionTime(() =>
{
	var total = 0;
	for (int i = 0; i < 10000; i++)
	{
		total += i;
		Console.WriteLine("Iteration: {0}, Running total: {1}", i, total);
	}
});

Console.WriteLine("Loop execution took: {0}", time);
Console.ReadKey();