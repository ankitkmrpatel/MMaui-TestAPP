using DevPocketCMS.Core.Interfaces;
using DevPocketCMS.Infrastructure.Data;
using DevPocketCMS.Infrastructure.Repositories;
using DevPocketCMS.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DevPocketCMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Database (singleton — one connection throughout app lifetime)
        services.AddSingleton<AppDatabase>();

        // Repositories (scoped — one per "request" / operation)
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IListRepository, ListRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IPreferencesRepository, PreferencesRepository>();

        // Services (transient — stateless, safe to create each time)
        services.AddTransient<ILinkPreviewService, LinkPreviewService>();
        services.AddTransient<IPocketModeService, PocketModeService>();
        services.AddTransient<INotificationService, NotificationService>();

        return services;
    }
}
