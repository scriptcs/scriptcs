using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Http.Dispatcher;

public class TestController : System.Web.Http.ApiController {

	public string Get() {
		return "Hello world!";
	}
}

public class ControllerResolver : DefaultHttpControllerTypeResolver {

    public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver) {

        var types = Assembly.GetExecutingAssembly().GetTypes();
        return types.Where(x => typeof(System.Web.Http.Controllers.IHttpController).IsAssignableFrom(x)).ToList();          
    }

}

var address = "http://localhost:8080";
var conf = new HttpSelfHostConfiguration(new Uri(address));
conf.Services.Replace(typeof(IHttpControllerTypeResolver), new ControllerResolver());

conf.Routes.MapHttpRoute(name: "DefaultApi",
   routeTemplate: "api/{controller}/{id}",
   defaults: new { id = RouteParameter.Optional }
);

Console.WriteLine(Assembly.GetAssembly(typeof(TestController)).FullName);	
Console.WriteLine(Assembly.GetAssembly(typeof(TestController)).IsDynamic);	

var server = new HttpSelfHostServer(conf);
server.OpenAsync().Wait();
Console.WriteLine("Listening...");
Console.ReadKey();