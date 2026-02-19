using System.Reflection;
using ERPNet.Api.Attributes;
using ERPNet.Api.Controllers.Common;
using ERPNet.Application.Common;
using ERPNet.Contracts;
using ERPNet.Domain.Common;
using ERPNet.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ERPNet.Testing.ArchTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly;
    private static readonly Assembly ContractsAssembly = typeof(Result).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(UsuarioService).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(DependencyInjection).Assembly;
    private static readonly Assembly ApiAssembly = typeof(BaseController).Assembly;
    private static readonly Assembly TestingAssembly = typeof(ArchitectureTests).Assembly;

    #region Dependencias entre capas

    [Fact(DisplayName = "Arch: Contracts no tiene dependencias externas")]
    public void Contracts_NoDependenciasExternas()
    {
        var referencias = ContractsAssembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => !n.StartsWith("System") && n != "mscorlib" && n != "netstandard")
            .ToList();

        Assert.True(referencias.Count == 0,
            $"Contracts tiene dependencias externas: {string.Join(", ", referencias)}");
    }

    [Fact(DisplayName = "Arch: Domain no referencia capas superiores")]
    public void Domain_NoReferenciaCapasSuperiores()
    {
        var prohibidos = new[] { "ERPNet.Application", "ERPNet.Infrastructure", "ERPNet.Api" };
        var referencias = DomainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name).ToList();

        var violaciones = prohibidos.Where(referencias.Contains).ToList();

        Assert.True(violaciones.Count == 0,
            $"Domain referencia: {string.Join(", ", violaciones)}");
    }

    [Fact(DisplayName = "Arch: Application no referencia Infrastructure ni Api")]
    public void Application_NoReferenciaInfrastructureNiApi()
    {
        var prohibidos = new[] { "ERPNet.Infrastructure", "ERPNet.Api" };
        var referencias = ApplicationAssembly.GetReferencedAssemblies()
            .Select(a => a.Name).ToList();

        var violaciones = prohibidos.Where(referencias.Contains).ToList();

        Assert.True(violaciones.Count == 0,
            $"Application referencia: {string.Join(", ", violaciones)}");
    }

    [Fact(DisplayName = "Arch: Infrastructure no referencia Api")]
    public void Infrastructure_NoReferenciaApi()
    {
        var referencias = InfrastructureAssembly.GetReferencedAssemblies()
            .Select(a => a.Name).ToList();

        Assert.DoesNotContain("ERPNet.Api", referencias);
    }

    #endregion

    #region Cobertura de tests

    [Fact(DisplayName = "Arch: Services de Application tienen tests")]
    public void ServicesDeApplication_TienenTests()
    {
        var serviceClasses = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"))
            .ToList();

        var testClassNames = TestingAssembly.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Tests"))
            .Select(t => t.Name)
            .ToHashSet();

        var sinTests = serviceClasses
            .Where(s => !testClassNames.Contains($"{s.Name}Tests"))
            .Select(s => s.Name)
            .ToList();

        Assert.True(sinTests.Count == 0,
            $"Services sin tests: {string.Join(", ", sinTests)}");
    }

    [Fact(DisplayName = "Arch: Controllers tienen tests")]
    public void Controllers_TienenTests()
    {
        var controllers = ApiAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                && t.Namespace == "ERPNet.Api.Controllers"
                && typeof(BaseController).IsAssignableFrom(t))
            .ToList();

        var testClassNames = TestingAssembly.GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Tests"))
            .Select(t => t.Name)
            .ToHashSet();

        var sinTests = controllers
            .Where(c => !testClassNames.Contains($"{c.Name}Tests"))
            .Select(c => c.Name)
            .ToList();

        Assert.True(sinTests.Count == 0,
            $"Controllers sin tests: {string.Join(", ", sinTests)}");
    }

    #endregion
}
