using System.Windows;
using System.Windows.Controls;
using ControlTaxi.Desktop.ViewModels;

namespace ControlTaxi.Desktop.Views;

public partial class UsuariosView : UserControl
{
    public UsuariosView()
    {
        InitializeComponent();
    }

    // PasswordBox no permite binding directo por seguridad; lo propagamos al VM.
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is UsuariosViewModel vm)
            vm.Password = ((PasswordBox)sender).Password;
    }
}
