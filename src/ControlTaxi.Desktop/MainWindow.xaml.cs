using System.Windows;
using ControlTaxi.Desktop.ViewModels;

namespace ControlTaxi.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
