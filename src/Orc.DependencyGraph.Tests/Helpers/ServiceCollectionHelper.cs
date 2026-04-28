namespace Orc.DependencyGraph.Tests;

using Catel;
using Microsoft.Extensions.DependencyInjection;
using Orc.DependencyGraph;

internal static class ServiceCollectionHelper
{
    public static IServiceCollection CreateServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();
        serviceCollection.AddCatelCore();
        serviceCollection.AddOrcDependencyGraph();

        return serviceCollection;
    }
}
