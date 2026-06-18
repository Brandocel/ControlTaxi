using System.IO;
using System.Windows;
using System.Windows.Threading;
using ControlTaxi.Application;
using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Desktop.Services;
using ControlTaxi.Desktop.ViewModels;
using ControlTaxi.Infrastructure;
using ControlTaxi.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ControlTaxi.Desktop;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // No apagar la app al cerrar la ventana de login; lo controlamos manualmente.
        // (Si no, WPF apaga al cerrar el login antes de mostrar el shell.)
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Logs en %LOCALAPPDATA%\ControlTaxi\logs\, rotación diaria, 30 días.
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ControlTaxi", "logs");
        Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(logDir, "controltaxi-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // Capturar TODA excepción no controlada para que nada se caiga en silencio.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log.Fatal(args.ExceptionObject as Exception, "Excepción no controlada en AppDomain.");
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Log.Error(args.Exception, "Excepción de Task no observada.");
            args.SetObserved();
        };

        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddApplication();
                    services.AddInfrastructure(context.Configuration);

                    // Sesión del usuario: el mismo singleton sirve como ICurrentUser.
                    services.AddSingleton<CurrentUserService>();
                    services.AddSingleton<ICurrentUser>(sp => sp.GetRequiredService<CurrentUserService>());

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<ShellViewModel>();
                    services.AddTransient<RelacionesViewModel>();
                    services.AddTransient<UsuariosViewModel>();
                    services.AddTransient<TaxistasViewModel>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            // Inicializar BD (migraciones + admin por defecto).
            using (var scope = _host.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                await initializer.InitializeAsync();
            }

            ShowLogin();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "No se pudo iniciar la aplicación.");
            MessageBox.Show(
                "No se pudo iniciar la aplicación. Revise la conexión a la base de datos en appsettings.json.\n\n" + ex.Message,
                "Error de inicio", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private void ShowLogin()
    {
        var login = _host!.Services.GetRequiredService<LoginWindow>();
        if (login.ShowDialog() == true)
        {
            var main = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = main;
            // A partir de aquí, cerrar la ventana principal sí cierra la app.
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
        }
        else
        {
            Shutdown();
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Excepción no controlada en la UI.");
        MessageBox.Show(
            "Ocurrió un error inesperado. Se registró en el log.\n\n" + e.Exception.Message,
            "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        await Log.CloseAndFlushAsync();
        base.OnExit(e);
    }
}
