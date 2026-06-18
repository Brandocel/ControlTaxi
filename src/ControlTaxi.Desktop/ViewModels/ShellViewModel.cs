using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlTaxi.Desktop.Services;
using ControlTaxi.Domain.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ControlTaxi.Desktop.ViewModels;

/// <summary>
/// ViewModel de la ventana principal (shell). Aloja el módulo activo en
/// <see cref="CurrentView"/> y expone comandos de navegación. El menú se muestra
/// según los permisos del usuario (el Administrador ve todo).
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly CurrentUserService _currentUser;
    private readonly IServiceProvider _services;

    public ShellViewModel(CurrentUserService currentUser, IServiceProvider services)
    {
        _currentUser = currentUser;
        _services = services;

        // Abrir el primer módulo permitido.
        if (PuedeVerRelaciones) NavegarRelaciones();
        else if (PuedeVerUsuarios) NavegarUsuarios();
        else NavegarDashboard();
    }

    public string UsuarioActual => _currentUser.NombreUsuario ?? "—";
    public string RolActual => _currentUser.Rol.ToString();

    // Visibilidad de módulos según permisos.
    public bool PuedeVerRelaciones => _currentUser.TienePermiso(Permisos.Relaciones);
    public bool PuedeVerUsuarios => _currentUser.TienePermiso(Permisos.Usuarios);

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

    [RelayCommand]
    private void NavegarUsuarios()
    {
        TituloModulo = "Usuarios, roles y permisos";
        CurrentView = _services.GetRequiredService<UsuariosViewModel>();
    }
}
