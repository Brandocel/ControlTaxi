namespace ControlTaxi.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de una relación ticket–taxista.
/// Refleja la máquina de estados del sistema anterior:
/// sin taxista → sin ticket POS → ligado → comisión pendiente → comisión pagada.
/// </summary>
public enum EstatusRelacion
{
    PendienteDeLigar = 0,
    PendienteDeTicket = 1,
    Ligado = 2,
    ComisionPendiente = 3,
    ComisionPagada = 4
}

/// <summary>Estado del importe "dejada" (lo que el taxista debe entregar).</summary>
public enum EstatusDejada
{
    SinImporte = 0,
    Pendiente = 1,
    Pagado = 2
}
