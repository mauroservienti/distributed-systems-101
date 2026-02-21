using System.Linq;
using System.Reflection;
using ServiceComposer.AspNetCore;
using Xunit;

namespace Demo.Tests;

/// <summary>
/// Smoke tests that verify all ViewModel Composition projects compile and their
/// ICompositionRequestsHandler implementations are accessible. Full end-to-end
/// tests require running database and HTTP back-end services.
/// </summary>
public class CompositionSmokeTests
{
    [Fact]
    public void All_viewmodel_composition_assemblies_contain_handlers()
    {
        var handlerType = typeof(ICompositionRequestsHandler);

        // Load each composition assembly and verify it contains at least one handler
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
