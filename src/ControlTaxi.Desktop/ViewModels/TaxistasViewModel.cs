using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Application.Features.Taxistas;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Desktop.ViewModels;

/// <summary>ViewModel del catálogo de Taxistas.</summary>
public sealed partial class TaxistasViewModel : ObservableObject
{
    private readonly ITaxistasService _service;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<TaxistasViewModel> _logger;

    public TaxistasViewModel(ITaxistasService service, ICurrentUser currentUser, ILogger<TaxistasViewModel> logger)
    {
        _service = service;
        _currentUser = currentUser;
        _logger = logger;
        _ = BuscarAsync();
    }

    public ObservableCollection<TaxistaRowDto> Taxistas { get; } = new();

    // --- Filtro ---
    [ObservableProperty] private string _busqueda = string.Empty;

    // --- Formulario ---
    [ObservableProperty] private string _clave = string.Empty;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _telefono = string.Empty;
    [ObservableProperty] private string _unidad = string.Empty;
    [ObservableProperty] private string _placas = string.Empty;
    [ObservableProperty] private bool _activo = true;
    [ObservableProperty] private bool _esNuevo = true;

    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task BuscarAsync()
    {
        IsBusy = true;
        Mensaje = null;
        try
        {
            var result = await _service.BuscarAsync(Busqueda);
            if (result.IsFailure) { Mensaje = result.Error.Message; return; }
            Taxistas.Clear();
            foreach (var t in result.Value) Taxistas.Add(t);
            Mensaje = $"{Taxistas.Count} taxista(s).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar taxistas.");
            Mensaje = "No se pudieron cargar los taxistas.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void Nuevo()
    {
        EsNuevo = true;
        Clave = Nombre = Telefono = Unidad = Placas = string.Empty;
        Activo = true;
        Mensaje = "Nuevo taxista.";
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        Mensaje = null;
        var request = new GuardarTaxistaRequest(Clave, Nombre, Telefono, Unidad, Placas, Activo);
        var result = await _service.GuardarAsync(request, _currentUser.NombreUsuario ?? "sistema");
        if (result.IsFailure) { Mensaje = result.Error.Message; return; }

        Mensaje = EsNuevo ? "Taxista creado." : "Taxista actualizado.";
        Nuevo();
        await BuscarAsync();
    }

    [RelayCommand]
    private void Editar(TaxistaRowDto? row)
    {
        if (row is null) return;
        EsNuevo = false;
        Clave = row.Clave;
        Nombre = row.Nombre;
        Telefono = row.Telefono;
        Unidad = row.Unidad;
        Placas = row.Placas;
        Activo = row.Activo;
        Mensaje = $"Editando {row.Clave}.";
    }

    /// <summary>Elimina sin confirmación; la confirmación la hace la vista.</summary>
    [RelayCommand]
    private async Task EliminarAsync(TaxistaRowDto? row)
    {
        if (row is null) return;
        Mensaje = null;
        var result = await _service.EliminarAsync(row.Id, _currentUser.NombreUsuario ?? "sistema");
        Mensaje = result.IsFailure ? result.Error.Message : $"Taxista {row.Clave} eliminado.";
        await BuscarAsync();
    }
}
