using Microsoft.Extensions.Logging;
using osobniSpravceFinanci.Services;

namespace osobniSpravceFinanci
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddSingleton<KategorieService>();
            builder.Services.AddTransient<KategoriePage>();
            builder.Services.AddSingleton<SablonyService>();
            builder.Services.AddTransient<SablonyPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
