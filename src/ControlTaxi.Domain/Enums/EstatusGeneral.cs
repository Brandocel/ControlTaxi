namespace ControlTaxi.Domain.Enums;

/// <summary>Estatus común para catálogos (taxistas, guías, usuarios, etc.).</summary>
public enum EstatusGeneral
{
    Inactivo = 0,
    Activo = 1
}

/// <summary>Roles del sistema. Los permisos finos se manejan por separado.</summary>
public enum RolUsuario
{
    Operador = 0,
    Supervisor = 1,
    Administrador = 2
}
