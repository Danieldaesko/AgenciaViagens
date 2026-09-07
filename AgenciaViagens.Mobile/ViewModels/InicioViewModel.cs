using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AgenciaViagens.Mobile.Models;
using AgenciaViagens.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgenciaViagens.Mobile.ViewModels;

public partial class InicioViewModel : BaseViewModel
{
    private readonly ApiService _api;

    public InicioViewModel(ApiService api) => _api = api;

    public ObservableCollection<PacoteModel> Destaques { get; } = new();
    public ObservableCollection<PacoteModel> Promocoes { get; } = new();

    [ObservableProperty] private bool temPromocoes;

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (Ocupado) return;

        Ocupado = true;
        MensagemErro = null;

        try
        {
            var destaques = await _api.ObterDestaquesAsync();
            Destaques.Clear();
            foreach (var p in destaques) Destaques.Add(p);

            var promocoes = await _api.ObterPromocoesAsync();
            Promocoes.Clear();
            foreach (var p in promocoes) Promocoes.Add(p);

            TemPromocoes = Promocoes.Count > 0;
        }
        catch (Exception)
        {
            MensagemErro = "Não foi possível carregar as viagens. Verifique a ligação.";
        }
        finally
        {
            Ocupado = false;
        }
    }

    [RelayCommand]
    private async Task AbrirPacoteAsync(PacoteModel pacote)
    {
        if (pacote is null) return;
        await Shell.Current.GoToAsync($"detalhepacote?id={pacote.Id}");
    }
}