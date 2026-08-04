using FluentAssertions;
using NetArchTest.Rules;

namespace AssignmentHub.ArchitectureTests;

public class LayerDependencyTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(AssignmentHub.Domain.Entities.User).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(AssignmentHub.Application.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(AssignmentHub.Infrastructure.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_ShouldNotDependOnApplication()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("AssignmentHub.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"the following types violate the rule: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Domain_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("AssignmentHub.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"the following types violate the rule: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Domain_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("AssignmentHub.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"the following types violate the rule: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("AssignmentHub.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"the following types violate the rule: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Application_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("AssignmentHub.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"the following types violate the rule: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Application_ShouldNotReferenceAspNetCore()
    {
        // The Application layer must stay framework-agnostic: it should never take a
        // compile-time dependency on ASP.NET Core types (controllers, HttpContext, etc.).
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"the following types violate the rule: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void Controllers_ShouldNotDependOnPersistence()
    {
        // Controllers must call Application-layer services, never the DbContext or EF
        // repositories directly.
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("AssignmentHub.Api.Controllers")
            .Should()
            .NotHaveDependencyOn("AssignmentHub.Infrastructure.Persistence")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"the following types violate the rule: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }
}
