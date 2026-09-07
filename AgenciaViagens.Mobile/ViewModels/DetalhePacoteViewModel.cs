using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Mobile.Models;
using AgenciaViagens.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Mobile.ViewModels;

[QueryProperty(nameof(PacoteId), "id")]
public partial class DetalhePacoteViewModel : BaseViewModel
{
    private readonly ApiService _api;

    public DetalhePacoteViewModel(ApiService api) => _api = api;

    [ObservableProperty] private int pacoteId;
    [ObservableProperty] private PacoteModel? pacote;
    [ObservableProperty] private bool temItinerario;

    public ObservableCollection<ItinerarioModel> Itinerario { get; } = new();

    partial void OnPacoteIdChanged(int value) => _ = CarregarAsync();

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (PacoteId <= 0) return;

        Ocupado = true;
        MensagemErro = null;

        try
        {
            Pacote = await _api.ObterPacoteAsync(PacoteId);

            Itinerario.Clear();
            if (Pacote?.Itinerarios is not null)
            {
                foreach (var i in Pacote.Itinerarios.OrderBy(x => x.Dia))
                    Itinerario.Add(i);
            }

            TemItinerario = Itinerario.Count > 0;
        }
        catch (Exception)
        {
            MensagemErro = "Não foi possível carregar esta viagem.";
        }
        finally
        {
            Ocupado = false;
        }
    }
}