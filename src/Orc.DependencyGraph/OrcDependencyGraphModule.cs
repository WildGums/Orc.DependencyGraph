namespace Orc.DependencyGraph
{
    using Catel.Services;
    using Catel.ThirdPartyNotices;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Core module which allows the registration of default services in the service collection.
    /// </summary>
    public static class OrcDependencyGraphModule
    {
        public static IServiceCollection AddOrcDependencyGraph(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILanguageSource>(new LanguageResourceSource("Orc.DependencyGraph", "Orc.DependencyGraph.Properties", "Resources"));

            serviceCollection.AddSingleton<IThirdPartyNotice>((x) => new LibraryThirdPartyNotice("Orc.DependencyGraph", "https://github.com/wildgums/orc.dependencygraph"));

            return serviceCollection;
        }
    }
}
