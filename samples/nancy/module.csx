public class IndexModule : NancyModule
{
	public IndexModule(IRootPathProvider provider)
	{
		Get["/"] = x => {
			return View["index"]; // "Nancy running on ScriptCS!";
		};
	}
}