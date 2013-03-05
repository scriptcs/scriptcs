using ServiceStack.ServiceInterface;

public class HelloService : Service
{
	public object Any(Hello request) 
	{
		return new HelloResponse { Result = "Hello, " + request.Name };
	}
}
