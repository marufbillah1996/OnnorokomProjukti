using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);

        // Entity <-> DTO mapping is done via explicit static extension methods per bounded
        // context (e.g. Assignments/Mappings/AssignmentMappingExtensions.cs), not AutoMapper —
        // avoids a runtime-reflection mapper, its recent commercial-licensing terms for larger
        // orgs, and keeps every mapping a plain, debuggable method.

        // Convention-based registration: every concrete "XService" implementing "IXService"
        // in this assembly is registered scoped. Keeps this file stable as bounded contexts grow.
        var serviceImplementations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.Name.EndsWith("Service", StringComparison.Ordinal));

        foreach (var implementationType in serviceImplementations)
        {
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

            if (interfaceType is not null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }

        return services;
    }
}
