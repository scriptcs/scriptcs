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


