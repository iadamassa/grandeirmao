using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using AnomaliaMonitor.Application.Services;

namespace AnomaliaMonitor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register application services
        services.AddScoped<ICrawlRequestService, CrawlRequestService>();
        
        return services;
    }
}