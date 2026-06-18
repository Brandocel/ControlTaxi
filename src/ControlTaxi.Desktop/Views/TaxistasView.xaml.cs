using System.Windows;
using System.Windows.Controls;
using ControlTaxi.Application.Features.Taxistas;
using ControlTaxi.Desktop.ViewModels;

namespace ControlTaxi.Desktop.Views;

public partial class TaxistasView : UserControl
{
    public TaxistasView()
    {
        InitializeComponent();
    }

    // La confirmación de borrado es responsabilidad de la vista (UI).
    private void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not TaxistaRowDto row) return;
        if (DataContext is not TaxistasViewModel vm) return;

        var r = MessageBox.Show(
            $"¿Eliminar al taxista {row.Clave} - {row.Nombre}?",
            "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (r == MessageBoxResult.Yes && vm.EliminarCommand.CanExecute(row))
            vm.EliminarCommand.Execute(row);
    }
}
