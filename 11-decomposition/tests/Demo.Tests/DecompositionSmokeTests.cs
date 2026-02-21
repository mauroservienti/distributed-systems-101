using System.Linq;
using System.Reflection;
using ServiceComposer.AspNetCore;
using Xunit;

namespace Demo.Tests;

/// <summary>
/// Smoke tests that verify all ViewModel Composition and Decomposition projects compile
/// and contain ICompositionRequestsHandler implementations. Full end-to-end tests require
/// running databases, NServiceBus, and HTTP back-end services.
/// </summary>
public class DecompositionSmokeTests
{
    [Fact]
    public void All_viewmodel_composition_assemblies_contain_handlers()
    {
        var handlerType = typeof(ICompositionRequestsHandler);

        var assemblyNames = new[]
        {
            "Marketing.ViewModelComposition",
            "Sales.ViewModelComposition",
            "Shipping.ViewModelComposition",
            "Warehouse.ViewModelComposition",
        };

        foreach (var assemblyName in assemblyNames)
        {
            var assembly = Assembly.Load(assemblyName);
            var handlers = assembly.GetTypes()
                .Where(t => handlerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
            Assert.NotEmpty(handlers);
        }
    }
}
