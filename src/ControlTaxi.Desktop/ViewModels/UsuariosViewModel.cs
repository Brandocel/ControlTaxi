using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Application.Features.Usuarios;
using ControlTaxi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Desktop.ViewModels;

public sealed record RolOption(RolUsuario Valor, string Texto);

/// <summary>ViewModel del módulo Usuarios, roles y permisos.</summary>
public sealed partial class UsuariosViewModel : ObservableObject
{
    private readonly IUsuariosService _service;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UsuariosViewModel> _logger;

    public UsuariosViewModel(IUsuariosService service, ICurrentUser currentUser, ILogger<UsuariosViewModel> logger)
    {
        _service = service;
        _currentUser = currentUser;
        _logger = logger;

        foreach (var m in _service.CatalogoPermisos())
            Permisos.Add(new PermisoItem(m.Clave, m.Nombre));

        _ = CargarAsync();
    }

    public ObservableCollection<UsuarioRowDto> Usuarios { get; } = new();
    public ObservableCollection<PermisoItem> Permisos { get; } = new();

    public IReadOnlyList<RolOption> Roles { get; } = new[]
    {
        new RolOption(RolUsuario.Operador, "Operador"),
        new RolOption(RolUsuario.Supervisor, "Supervisor"),
        new RolOption(RolUsuario.Administrador, "Administrador"),
    };

    [ObservableProperty] private string _nombreUsuario = string.Empty;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private RolOption? _rolSeleccionado;
    [ObservableProperty] private bool _activo = true;
    [ObservableProperty] private bool _esNuevo = true;
    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private UsuarioRowDto? _usuarioSeleccionado;

    partial void OnUsuarioSeleccionadoChanged(UsuarioRowDto? value)
    {
        if (value is null) return;
        EsNuevo = false;
        NombreUsuario = value.NombreUsuario;
        Nombre = value.Nombre;
        Password = string.Empty;
        Activo = value.Activo;
        RolSeleccionado = Roles.FirstOrDefault(r => r.Valor == value.Rol);
        foreach (var p in Permisos)
            p.Seleccionado = value.Permisos.Contains(p.Clave, StringComparer.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private async Task CargarAsync()
    {
        IsBusy = true;
        Mensaje = null;
        try
        {
            var result = await _service.ListarAsync();
            if (result.IsFailure) { Mensaje = result.Error.Message; return; }
            Usuarios.Clear();
            foreach (var u in result.Value) Usuarios.Add(u);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar usuarios.");
            Mensaje = "No se pudieron cargar los usuarios.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Nuevo()
    {
        EsNuevo = true;
        UsuarioSeleccionado = null;
        NombreUsuario = Nombre = Password = string.Empty;
        Activo = true;
        RolSeleccionado = Roles.First(r => r.Valor == RolUsuario.Operador);
        foreach (var p in Permisos) p.Seleccionado = false;
        Mensaje = "Nuevo usuario.";
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        Mensaje = null;
        var rol = RolSeleccionado?.Valor ?? RolUsuario.Operador;
        var permisos = Permisos.Where(p => p.Seleccionado).Select(p => p.Clave).ToList();

        var request = new GuardarUsuarioRequest(
            NombreUsuario: NombreUsuario,
            Nombre: Nombre,
            Password: string.IsNullOrWhiteSpace(Password) ? null : Password,
            Rol: rol,
            Activo: Activo,
            Permisos: permisos);

        var result = await _service.GuardarAsync(request, _currentUser.NombreUsuario ?? "sistema");
        if (result.IsFailure) { Mensaje = result.Error.Message; return; }

        Mensaje = EsNuevo ? "Usuario creado." : "Usuario actualizado.";
        await CargarAsync();
    }
}
