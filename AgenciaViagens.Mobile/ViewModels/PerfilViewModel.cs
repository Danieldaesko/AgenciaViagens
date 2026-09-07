using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgenciaViagens.Mobile.ViewModels;

public partial class PerfilViewModel : BaseViewModel
{
    private readonly TokenService _tokens;

    public PerfilViewModel(TokenService tokens) => _tokens = tokens;

    [ObservableProperty] private string nome = "";
    [ObservableProperty] private string email = "";

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Nome = await _tokens.ObterNomeAsync() ?? "";
        Email = await _tokens.ObterEmailAsync() ?? "";
    }

    [RelayCommand]
    private async Task SairAsync()
    {
        var confirmar = await Shell.Current.DisplayAlert(
            "Terminar sessão", "Quer sair da sua conta?", "Sair", "Ficar");

        if (!confirmar) return;

        _tokens.TerminarSessao();
        await Shell.Current.GoToAsync("//login");
    }
}