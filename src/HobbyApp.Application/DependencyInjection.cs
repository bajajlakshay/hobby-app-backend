using HobbyApp.Application.Notes;
using HobbyApp.Application.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace HobbyApp.Application;

/// <summary>
/// Registers the Application layer's services (use cases, validators, mappers, ...).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
