using DevPocketCMS.Application;
using DevPocketCMS.Infrastructure;
using DevPocketCMS.Infrastructure.Data;
using MauiReactor;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace DevPocketCMS;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiReactorApp<AppRoot>(app =>
            {
                // No XAML resources — all styling is done in C# via MauiReactor
            })
            .UseLocalNotification()
#if DEBUG
            //.EnableMauiReactorHotReload()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ── Dependency Injection ──────────────────────────────────────────────
        builder.Services.AddInfrastructure()
            .AddApplication();

        var app = builder.Build();

        return app;
    }
}
