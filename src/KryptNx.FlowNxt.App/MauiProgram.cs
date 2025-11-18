using MauiReactor;
using Microsoft.Extensions.DependencyInjection;

namespace KryptNx.FlowNxt.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiReactorApp<MainPage>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Rubik-Bold.ttf", "RubikBold");
                    fonts.AddFont("Rubik-Light.ttf", "RubikLight");
                    fonts.AddFont("Rubik-Medium.ttf", "RubikMedium");
                    fonts.AddFont("Rubik-Regular.ttf", "RubikRegular");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("fa-solid-900.ttf", "FA");           // FontAwesome (solid) alias "FA"
                    fonts.AddFont("fa-brands-400.ttf", "FABrand");     // optional
                    fonts.AddFont("fa-regular-400.ttf", "FAReg");      // optional
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", "Fluent");
                    fonts.AddFont("FluentSystemIcons.ttf", FluentUI.FontFamily); // Fluent icons alias
                });

#if DEBUG
    		//builder.Logging.AddDebug();
    		//builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddSingleton<ModalErrorHandler>();

            return builder.Build();
        }
    }
}
