using AgenciaViagens.Mobile.ViewModels;

namespace AgenciaViagens.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}