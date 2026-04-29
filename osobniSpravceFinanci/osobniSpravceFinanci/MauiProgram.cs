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

            // reistrace sluzeb (napojeni na databazi)
            // jedna instance ktera bezi po celou dobu
            builder.Services.AddSingleton<TransakceService>();
            builder.Services.AddSingleton<KategorieService>();
            builder.Services.AddSingleton<SablonyService>();
            builder.Services.AddSingleton<CileService>();

            // registrace gui stranek
            // vytvori se vzdy nova instance
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<KategoriePage>();
            builder.Services.AddTransient<SablonyPage>();
            builder.Services.AddTransient<CilePage>();
            builder.Services.AddTransient<StatistikyPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
