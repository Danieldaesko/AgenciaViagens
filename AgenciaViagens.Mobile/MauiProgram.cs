using AgenciaViagens.Mobile.Services;
using Microsoft.Extensions.Logging;
using AgenciaViagens.Mobile.Pages;
using AgenciaViagens.Mobile.ViewModels;


namespace AgenciaViagens.Mobile;

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

        // Serviços
        builder.Services.AddSingleton<TokenService>();
        builder.Services.AddSingleton<ApiService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<InicioViewModel>();
        builder.Services.AddTransient<PacotesViewModel>();
        builder.Services.AddTransient<DetalhePacoteViewModel>();
        builder.Services.AddTransient<ReservasViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();

        // Páginas
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<InicioPage>();
        builder.Services.AddTransient<PacotesPage>();
        builder.Services.AddTransient<DetalhePacotePage>();
        builder.Services.AddTransient<ReservasPage>();
        builder.Services.AddTransient<PerfilPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
