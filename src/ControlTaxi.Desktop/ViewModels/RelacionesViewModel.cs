using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlTaxi.Application.Common.Interfaces;
using ControlTaxi.Application.Features.Relaciones;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Desktop.ViewModels;

/// <summary>ViewModel del módulo Relaciones ticket–taxista.</summary>
public sealed partial class RelacionesViewModel : ObservableObject
{
    private readonly IRelacionesService _service;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<RelacionesViewModel> _logger;

    public RelacionesViewModel(IRelacionesService service, ICurrentUser currentUser, ILogger<RelacionesViewModel> logger)
    {
        _service = service;
        _currentUser = currentUser;
        _logger = logger;
    }

    public ObservableCollection<RelacionRowDto> Relaciones { get; } = new();
    public ObservableCollection<TaxistaOption> TaxistaOpciones { get; } = new();

    // --- Filtros ---
    [ObservableProperty] private string _busqueda = string.Empty;
    [ObservableProperty] private DateTime? _fechaInicio = DateTime.Today;
    [ObservableProperty] private DateTime? _fechaFin = DateTime.Today;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PagarDejadaCommand))]
    private RelacionRowDto? _relacionSeleccionada;

    // --- Formulario para ligar / guardar ---
    [ObservableProperty] private string _folioApp = string.Empty;
    [ObservableProperty] private string _folioOperacion = string.Empty;
    [ObservableProperty] private string _folioPos = string.Empty;
    [ObservableProperty] private string _gafete = string.Empty;
    [ObservableProperty] private string _transporteTipo = string.Empty;
    [ObservableProperty] private string _observaciones = string.Empty;
    [ObservableProperty] private string _taxistaQuery = string.Empty;
    [ObservableProperty] private TaxistaOption? _taxistaSeleccionado;

    [ObservableProperty] private string? _mensaje;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task BuscarAsync()
    {
        IsBusy = true;
        Mensaje = null;
        try
        {
            var result = await _service.BuscarAsync(new RelacionesFiltro(Busqueda, FechaInicio, FechaFin));
            if (result.IsFailure) { Mensaje = result.Error.Message; return; }

            Relaciones.Clear();
            foreach (var fila in result.Value)
                Relaciones.Add(fila);
            Mensaje = $"{Relaciones.Count} relación(es).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar relaciones.");
            Mensaje = "No se pudieron cargar las relaciones.";
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task BuscarTaxistasAsync()
    {
        var result = await _service.BuscarTaxistasAsync(TaxistaQuery);
        if (result.IsFailure) return;
        TaxistaOpciones.Clear();
        foreach (var t in result.Value)
            TaxistaOpciones.Add(t);
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        Mensaje = null;
        var request = new GuardarRelacionRequest(
            FolioApp: FolioApp,
            FolioOperacion: FolioOperacion,
            FolioPos: FolioPos,
            Gafete: Gafete,
            TaxistaId: TaxistaSeleccionado?.Id ?? 0,
            TaxistaNombre: TaxistaSeleccionado?.Nombre,
            TransporteTipo: TransporteTipo,
            Observaciones: Observaciones);

        var result = await _service.GuardarAsync(request, _currentUser.NombreUsuario ?? "sistema");
        if (result.IsFailure) { Mensaje = result.Error.Message; return; }

        Mensaje = "Relación guardada.";
        LimpiarFormulario();
        await BuscarAsync();
    }

    private bool PuedePagarDejada() => RelacionSeleccionada?.PuedePagarDejada == true;

    [RelayCommand(CanExecute = nameof(PuedePagarDejada))]
    private async Task PagarDejadaAsync()
    {
        if (RelacionSeleccionada is null) return;
        Mensaje = null;

        var result = await _service.PagarDejadaAsync(
            new PagarDejadaRequest(RelacionSeleccionada.Id, RelacionSeleccionada.FolioPos),
            _currentUser.NombreUsuario ?? "sistema");

        Mensaje = result.IsFailure ? result.Error.Message : "Dejada cobrada.";
        await BuscarAsync();
    }

    private void LimpiarFormulario()
    {
        FolioApp = FolioOperacion = FolioPos = Gafete = TransporteTipo = Observaciones = TaxistaQuery = string.Empty;
        TaxistaSeleccionado = null;
        TaxistaOpciones.Clear();
    }
}
