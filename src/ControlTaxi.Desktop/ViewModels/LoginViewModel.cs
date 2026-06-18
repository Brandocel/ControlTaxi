using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlTaxi.Application.Features.Auth;
using ControlTaxi.Desktop.Services;
using Microsoft.Extensions.Logging;

namespace ControlTaxi.Desktop.ViewModels;

public sealed partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly CurrentUserService _currentUser;
    private readonly ILogger<LoginViewModel> _logger;

    public LoginViewModel(IAuthService authService, CurrentUserService currentUser, ILogger<LoginViewModel> logger)
    {
        _authService = authService;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Se dispara cuando el login es correcto, para que la UI navegue al shell.</summary>
    public event EventHandler? LoginSucceeded;

    [ObservableProperty] private string _nombreUsuario = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private bool _isBusy;

    private bool CanLogin() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var result = await _authService.LoginAsync(new LoginRequest(NombreUsuario, Password));
            if (result.IsFailure)
            {
                ErrorMessage = result.Error.Message;
                return;
            }

            _currentUser.SetSession(result.Value);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado durante el login.");
            ErrorMessage = "No se pudo conectar con la base de datos. Revise la configuración.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
