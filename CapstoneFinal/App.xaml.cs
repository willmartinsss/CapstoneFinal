using SpaceInvaders.Views;
using SpaceInvaders.ViewModels;
using SpaceInvaders.Services;
using Uno.Resizetizer;

namespace CapstoneFinal;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder.EmbeddedSource<App>())
                .UseLocalization()
                .UseHttp((context, services) =>
                {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    // Register game services
                    services.AddSingleton<AudioService>();
                    services.AddSingleton<GameLogicService>();
                    services.AddSingleton<ScoreService>();
                    
                    // Register ViewModels
                    services.AddTransient<GameViewModel>();
                    services.AddTransient<MenuViewModel>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        
        // Navigate to the main page
        Host = await builder.NavigateAsync<MainPage>();
    }
}
