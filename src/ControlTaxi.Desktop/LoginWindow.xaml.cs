using System.Windows;
using System.Windows.Controls;
using ControlTaxi.Desktop.ViewModels;

namespace ControlTaxi.Desktop;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.LoginSucceeded += OnLoginSucceeded;
        Loaded += (_, _) => UsuarioBox.Focus();
    }

    // PasswordBox no permite binding directo por seguridad; lo propagamos al VM.
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = ((PasswordBox)sender).Password;
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
