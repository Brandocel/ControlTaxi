using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlTaxi.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ControlTaxi.Desktop.ViewModels;

/// <summary>
/// ViewModel de la ventana principal (shell). Aloja el módulo activo en
/// <see cref="CurrentView"/> y expone comandos de navegación. Los ViewModels
/// se resuelven del contenedor DI bajo demanda.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly CurrentUserService _currentUser;
    private readonly IServiceProvider _services;

    public ShellViewModel(CurrentUserService currentUser, IServiceProvider services)
    {
        _currentUser = currentUser;
        _services = services;
        NavegarRelaciones();
    }

    public string UsuarioActual => _currentUser.NombreUsuario ?? "—";

    [ObservableProperty] private string _tituloModulo = "Dashboard";
    [ObservableProperty] private object? _currentView;

    [RelayCommand]
    private void NavegarDashboard()
    {
        TituloModulo = "Dashboard";
        CurrentView = null;
    }

    [RelayCommand]
    private void NavegarRelaciones()
    {
        TituloModulo = "Relaciones ticket–taxista";
        CurrentView = _services.GetRequiredService<RelacionesViewModel>();
    }
}
