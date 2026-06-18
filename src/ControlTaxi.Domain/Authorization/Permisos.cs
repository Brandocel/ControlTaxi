namespace ControlTaxi.Domain.Authorization;

/// <summary>Un módulo del sistema sobre el que se concede permiso "puede ver".</summary>
public sealed record ModuloPermiso(string Clave, string Nombre);

/// <summary>
/// Catálogo central de permisos por módulo. Es la única fuente de verdad de
/// "qué se puede ver" en el sistema. El rol Administrador los tiene todos.
/// </summary>
public static class Permisos
{
    public const string RegistroDiario = "RegistroDiario";
    public const string Comisiones = "Comisiones";
    public const string Transportes = "Transportes";
    public const string Guias = "Guias";
    public const string Taxistas = "Taxistas";
    public const string Gafetes = "Gafetes";
    public const string Relaciones = "Relaciones";
    public const string Gastos = "Gastos";
    public const string Cortes = "Cortes";
    public const string Reportes = "Reportes";
    public const string ReporteTaxis = "ReporteTaxis";
    public const string ControlDejadas = "ControlDejadas";
    public const string DejadasComisiones = "DejadasComisiones";
    public const string ConcentradoGeneral = "ConcentradoGeneral";
    public const string Usuarios = "Usuarios";

    /// <summary>Lista de módulos disponibles para asignar permisos.</summary>
    public static readonly IReadOnlyList<ModuloPermiso> Catalogo = new List<ModuloPermiso>
    {
        new(RegistroDiario, "Registro diario"),
        new(Comisiones, "Comisiones"),
        new(Transportes, "Transportes"),
        new(Guias, "Guías"),
        new(Taxistas, "Taxistas"),
        new(Gafetes, "Gafetes"),
        new(Relaciones, "Relación ticket–taxista"),
        new(Gastos, "Gastos"),
        new(Cortes, "Cortes"),
        new(Reportes, "Reportes"),
        new(ReporteTaxis, "Reporte taxis"),
        new(ControlDejadas, "Control dejadas"),
        new(DejadasComisiones, "Dejadas y comisiones"),
        new(ConcentradoGeneral, "Concentrado general"),
        new(Usuarios, "Usuarios"),
    };

    private static readonly HashSet<string> Validos =
        new(Catalogo.Select(m => m.Clave), StringComparer.OrdinalIgnoreCase);

    /// <summary>Filtra una lista de permisos dejando solo los del catálogo.</summary>
    public static IEnumerable<string> SoloValidos(IEnumerable<string> permisos) =>
        permisos.Where(p => Validos.Contains(p));
}
