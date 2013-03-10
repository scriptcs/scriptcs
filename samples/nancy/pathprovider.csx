public class PathProvider : IRootPathProvider
{
	public string GetRootPath()
	{
		return Path.Combine("..\\..\\", Environment.CurrentDirectory);
	}
}