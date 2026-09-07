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

public partial class PacotesViewModel : BaseViewModel
{
    private readonly ApiService _api;
    private List<PacoteModel> _todos = new();

    public PacotesViewModel(ApiService api) => _api = api;

    public ObservableCollection<PacoteModel> Pacotes { get; } = new();

    [ObservableProperty] private string termoPesquisa = "";
    [ObservableProperty] private bool semResultados;

    partial void OnTermoPesquisaChanged(string value) => Filtrar();

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (Ocupado) return;

        Ocupado = true;
        MensagemErro = null;

        try
        {
            _todos = await _api.ObterTodosPacotesAsync();
            Filtrar();
        }
        catch (Exception)
        {
            MensagemErro = "Não foi possível carregar os pacotes.";
        }
        finally
        {
            Ocupado = false;
        }
    }

    private void Filtrar()
    {
        var termo = TermoPesquisa?.Trim() ?? "";

        var filtrados = string.IsNullOrEmpty(termo)
            ? _todos
            : _todos.Where(p =>
                p.Destino.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                p.Pais.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                p.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                p.Origem.Contains(termo, StringComparison.OrdinalIgnoreCase)).ToList();

        Pacotes.Clear();
        foreach (var p in filtrados) Pacotes.Add(p);

        SemResultados = Pacotes.Count == 0 && !Ocupado;
    }

    [RelayCommand]
    private async Task AbrirPacoteAsync(PacoteModel pacote)
    {
        if (pacote is null) return;
        await Shell.Current.GoToAsync($"detalhepacote?id={pacote.Id}");
    }
}