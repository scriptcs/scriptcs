using System.Diagnostics;

public class PerformanceUtilities
{
	public static long ExecutionTime(Action action)
	{
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                action();

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
	}
}