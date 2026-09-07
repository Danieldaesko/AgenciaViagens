using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgenciaViagens.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly ApiService _api;

    public LoginViewModel(ApiService api) => _api = api;

    [ObservableProperty] private string email = "";
    [ObservableProperty] private string password = "";

    [RelayCommand]
    private async Task EntrarAsync()
    {
        MensagemErro = null;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            MensagemErro = "Preencha o email e a palavra-passe.";
            return;
        }

        Ocupado = true;
        try
        {
            var resultado = await _api.LoginAsync(Email.Trim(), Password);

            if (!resultado.Sucesso)
            {
                MensagemErro = resultado.Mensagem ?? "Não foi possível entrar.";
                return;
            }

            Password = "";
            await Shell.Current.GoToAsync("//inicio");
        }
        finally
        {
            Ocupado = false;
        }
    }
}