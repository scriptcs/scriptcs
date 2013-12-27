#load event.csx

public class TestController : System.Web.Http.ApiController {

	public string Get() {
		EventHelper.FinishEvent.Set();
		return "Hello world!";
	}
}