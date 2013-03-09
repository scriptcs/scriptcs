public class Bootstrapper : AutofacNancyBootstrapper
{
    protected override IEnumerable<ModuleRegistration> Modules
    {
        get
        {
            return new [] {
                new ModuleRegistration(typeof(IndexModule), typeof(IndexModule).FullName)
            };
        }
    }

    protected override NancyInternalConfiguration InternalConfiguration
    {
        get
        {
            return NancyInternalConfiguration.WithOverrides(x => x.RouteDescriptionProvider = typeof(CustomRouteDescriptionProvider));
        }
    }

    protected override IRootPathProvider RootPathProvider
    {
        get { return new PathProvider(); }
    }
}