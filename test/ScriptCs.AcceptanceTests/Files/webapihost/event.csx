using System.Threading;

public static class EventHelper 
{
	public static ManualResetEventSlim FinishEvent = new ManualResetEventSlim(false);	
}