using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using AutoMapper;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
