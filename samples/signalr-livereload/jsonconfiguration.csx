public class JsonConfiguration
{
    public static bool Exists(string name)
    {
        return File.Exists(name);
    }

    public static TConfigFile Get<TConfigFile>(string name)
    {
        var contents = File.ReadAllText(name);
        var config = JsonConvert.DeserializeObject<TConfigFile>(contents);
        return config;
    }

    public static void Put<TConfigFile>(string name, TConfigFile instance)
    {
        var contents = JsonConvert.SerializeObject(instance, Formatting.Indented);
        File.WriteAllText(name, contents);
    }
}