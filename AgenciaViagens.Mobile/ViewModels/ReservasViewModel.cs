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

public partial class ReservasViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private readonly TokenService _tokens;

    public ReservasViewModel(ApiService api, TokenService tokens)
    {
        _api = api;
        _tokens = tokens;
    }

    public ObservableCollection<ReservaModel> Reservas { get; } = new();

    [ObservableProperty] private bool semReservas;

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (Ocupado) return;

        if (!await _tokens.EstaAutenticadoAsync())
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        Ocupado = true;
        MensagemErro = null;

        try
        {
            var lista = await _api.ObterMinhasReservasAsync();
            Reservas.Clear();
            foreach (var r in lista) Reservas.Add(r);

            SemReservas = Reservas.Count == 0;
        }
        catch (Exception)
        {
            MensagemErro = "Não foi possível carregar as reservas.";
        }
        finally
        {
            Ocupado = false;
        }
    }

    [RelayCommand]
    private async Task CancelarAsync(ReservaModel reserva)
    {
        if (reserva is null) return;

        var confirmar = await Shell.Current.DisplayAlert(
            "Cancelar reserva",
            $"Quer mesmo cancelar a reserva {reserva.Referencia}?",
            "Sim, cancelar", "Voltar atrás");

        if (!confirmar) return;

        Ocupado = true;
        try
        {
            var (sucesso, mensagem) = await _api.CancelarReservaAsync(reserva.Id);
            await Shell.Current.DisplayAlert(
                sucesso ? "Cancelada" : "Não foi possível", mensagem, "OK");

            if (sucesso) await CarregarAsync();
        }
        finally
        {
            Ocupado = false;
        }
    }
}