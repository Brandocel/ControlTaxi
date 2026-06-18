using CommunityToolkit.Mvvm.ComponentModel;

namespace ControlTaxi.Desktop.ViewModels;

/// <summary>Item de permiso con casilla (para la lista de módulos en Usuarios).</summary>
public sealed partial class PermisoItem : ObservableObject
{
    public PermisoItem(string clave, string nombre)
    {
        Clave = clave;
        Nombre = nombre;
    }

    public string Clave { get; }
    public string Nombre { get; }

    [ObservableProperty] private bool _seleccionado;
}
