using DevPocketCMS.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevPocketCMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ArticleService>();
        services.AddScoped<ListService>();
        services.AddScoped<TagService>();
        services.AddScoped<SearchService>();
        services.AddScoped<DashboardService>();
        
        return services;
    }
}
