using ControlTaxi.Domain.Common;
using ControlTaxi.Domain.Enums;

namespace ControlTaxi.Domain.Entities;

/// <summary>
/// Relación entre un ticket de la app móvil (<see cref="FolioApp"/>), un ticket
/// del POS (<see cref="FolioOperacion"/>) y el taxista que realizó el viaje.
/// Es el corazón del módulo "Relaciones ticket–taxista".
///
/// Toda la lógica de estado vive aquí (dominio rico), no dispersa en servicios:
/// la máquina de estados <see cref="Estatus"/>, el estado de la dejada y el pago.
/// </summary>
public class RelacionTicketTaxista : AuditableEntity
{
    // --- Identificación del viaje ---
    public string FolioApp { get; private set; } = string.Empty;     // ticket de la app móvil (clave de negocio)
    public string FolioOperacion { get; private set; } = string.Empty; // ticket del POS
    public string FolioPos { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }

    // --- Datos del viaje (snapshot) ---
    public string Hotel { get; private set; } = string.Empty;
    public string Origen { get; private set; } = string.Empty;
    public string Destino { get; private set; } = string.Empty;
    public string Nacionalidad { get; private set; } = string.Empty;
    public int Pax { get; private set; }

    // --- Taxista ligado ---
    public int? TaxistaId { get; private set; }
    public string TaxistaNombre { get; private set; } = string.Empty;
    public string TransporteTipo { get; private set; } = string.Empty;
    public string Gafete { get; private set; } = string.Empty;
    public string Unidad { get; private set; } = string.Empty;
    public string Placas { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public string Observaciones { get; private set; } = string.Empty;

    // --- Importes ---
    public decimal Venta { get; private set; }
    public decimal Dejada { get; private set; }
    public decimal DejadaPagada { get; private set; }
    public decimal Comision { get; private set; }
    public decimal Pago { get; private set; }

    // --- Pago de dejada ---
    public DateTime? FechaPagoDejada { get; private set; }
    public string UsuarioPagoDejada { get; private set; } = string.Empty;
    public string TicketPagoDejada { get; private set; } = string.Empty;

    private RelacionTicketTaxista() { }

    public RelacionTicketTaxista(string folioApp, DateTime fecha)
    {
        if (string.IsNullOrWhiteSpace(folioApp))
            throw new ArgumentException("El folio de la app (ticket) es obligatorio.", nameof(folioApp));
        FolioApp = folioApp.Trim();
        Fecha = fecha;
    }

    /// <summary>Estado del importe que el taxista debe entregar (dejada).</summary>
    public EstatusDejada EstatusDejada =>
        Dejada <= 0m && DejadaPagada <= 0m ? EstatusDejada.SinImporte
        : DejadaPagada > 0m ? EstatusDejada.Pagado
        : EstatusDejada.Pendiente;

    /// <summary>Solo se puede cobrar una dejada con importe pendiente y no pagada aún.</summary>
    public bool PuedePagarDejada => Dejada > 0m && DejadaPagada <= 0m && EstatusDejada != EstatusDejada.Pagado;

    /// <summary>
    /// Máquina de estados de la relación (idéntica a la del sistema anterior):
    /// sin taxista → sin ticket POS → comisión pagada/pendiente → ligado.
    /// </summary>
    public EstatusRelacion Estatus =>
        TaxistaId is null or <= 0 ? EstatusRelacion.PendienteDeLigar
        : string.IsNullOrWhiteSpace(FolioOperacion) ? EstatusRelacion.PendienteDeTicket
        : Pago > 0m ? EstatusRelacion.ComisionPagada
        : Comision > 0m ? EstatusRelacion.ComisionPendiente
        : EstatusRelacion.Ligado;

    /// <summary>Liga (asigna) un taxista a la relación.</summary>
    public void LigarTaxista(int taxistaId, string taxistaNombre, string? transporteTipo = null,
        string? gafete = null, string? observaciones = null)
    {
        if (taxistaId <= 0)
            throw new ArgumentException("El taxista a ligar no es válido.", nameof(taxistaId));
        TaxistaId = taxistaId;
        TaxistaNombre = (taxistaNombre ?? string.Empty).Trim();
        if (transporteTipo is not null) TransporteTipo = transporteTipo.Trim();
        if (gafete is not null) Gafete = gafete.Trim();
        if (observaciones is not null) Observaciones = observaciones.Trim();
    }

    /// <summary>Asocia el ticket del POS a la relación.</summary>
    public void AsignarTicketPos(string folioOperacion, string? folioPos = null)
    {
        FolioOperacion = (folioOperacion ?? string.Empty).Trim();
        if (folioPos is not null) FolioPos = folioPos.Trim();
    }

    /// <summary>Actualiza los campos descriptivos (transporte, gafete, observaciones).</summary>
    public void ActualizarDescriptivos(string? transporteTipo, string? gafete, string? observaciones)
    {
        if (transporteTipo is not null) TransporteTipo = transporteTipo.Trim();
        if (gafete is not null) Gafete = gafete.Trim();
        if (observaciones is not null) Observaciones = observaciones.Trim();
    }

    public void EstablecerDatosViaje(string? hotel, string? origen, string? destino, string? nacionalidad, int pax,
        string? unidad = null, string? placas = null, string? telefono = null)
    {
        Hotel = (hotel ?? string.Empty).Trim();
        Origen = (origen ?? string.Empty).Trim();
        Destino = (destino ?? string.Empty).Trim();
        Nacionalidad = (nacionalidad ?? string.Empty).Trim();
        Pax = pax < 0 ? 0 : pax;
        if (unidad is not null) Unidad = unidad.Trim();
        if (placas is not null) Placas = placas.Trim();
        if (telefono is not null) Telefono = telefono.Trim();
    }

    public void EstablecerImportes(decimal venta, decimal dejada, decimal comision)
    {
        Venta = venta < 0m ? 0m : venta;
        Dejada = dejada < 0m ? 0m : dejada;
        Comision = comision < 0m ? 0m : comision;
    }

    /// <summary>Registra la comisión ya pagada al taxista.</summary>
    public void RegistrarPagoComision(decimal pago)
    {
        if (pago < 0m)
            throw new ArgumentException("El pago de comisión no puede ser negativo.", nameof(pago));
        Pago = pago;
    }

    /// <summary>
    /// Cobra la dejada completa. Marca el importe como pagado, guarda quién, cuándo
    /// y con qué ticket. Equivale al UPDATE dejadas SET pago=total del sistema viejo.
    /// </summary>
    public void PagarDejada(string usuario, string? ticket)
    {
        if (!PuedePagarDejada)
            throw new InvalidOperationException("La dejada no tiene importe pendiente o ya fue pagada.");

        DejadaPagada = Dejada;
        FechaPagoDejada = DateTime.Now;
        UsuarioPagoDejada = (usuario ?? string.Empty).Trim();
        TicketPagoDejada = (ticket ?? string.Empty).Trim();
    }
}
