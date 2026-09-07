using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AgenciaViagens.Mobile.Models;

namespace AgenciaViagens.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly TokenService _tokenService;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(TokenService tokenService)
    {
        _tokenService = tokenService;

        var handler = new HttpClientHandler();

#if DEBUG
        // Aceita o certificado self-signed do localhost em desenvolvimento
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif

        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private async Task PrepararAuthAsync()
    {
        var token = await _tokenService.ObterTokenAsync();
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    // ── AUTENTICAÇÃO ──────────────────────────────────

    public async Task<LoginResposta> LoginAsync(string email, string password)
    {
        try
        {
            var resposta = await _http.PostAsJsonAsync("auth/login",
                new LoginPedido { Email = email, Password = password });

            if (!resposta.IsSuccessStatusCode)
                return new LoginResposta { Sucesso = false, Mensagem = "Email ou palavra-passe incorretos." };

            var resultado = await resposta.Content.ReadFromJsonAsync<LoginResposta>(JsonOpts);

            if (resultado?.Token is not null)
            {
                await _tokenService.GuardarSessaoAsync(
                    resultado.Token, resultado.UtilizadorId,
                    resultado.Nome ?? "", resultado.Email ?? "");
            }

            return resultado ?? new LoginResposta { Sucesso = false, Mensagem = "Resposta inválida." };
        }
        catch (Exception)
        {
            return new LoginResposta { Sucesso = false, Mensagem = "Não foi possível ligar ao servidor." };
        }
    }

    // ── PACOTES ───────────────────────────────────────

    public async Task<List<PacoteModel>> ObterDestaquesAsync()
    {
        var resposta = await _http.GetAsync("pacote/destaques");
        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadFromJsonAsync<List<PacoteModel>>(JsonOpts) ?? new();
    }

    public async Task<List<PacoteModel>> ObterPromocoesAsync()
    {
        var resposta = await _http.GetAsync("pacote/promocoes");
        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadFromJsonAsync<List<PacoteModel>>(JsonOpts) ?? new();
    }

    public async Task<List<PacoteModel>> ObterTodosPacotesAsync()
    {
        var resposta = await _http.GetAsync("pacote");
        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadFromJsonAsync<List<PacoteModel>>(JsonOpts) ?? new();
    }

    public async Task<PacoteModel?> ObterPacoteAsync(int id)
    {
        var resposta = await _http.GetAsync($"pacote/{id}");
        if (!resposta.IsSuccessStatusCode) return null;
        return await resposta.Content.ReadFromJsonAsync<PacoteModel>(JsonOpts);
    }

    // ── RESERVAS ──────────────────────────────────────

    public async Task<List<ReservaModel>> ObterMinhasReservasAsync()
    {
        await PrepararAuthAsync();
        var resposta = await _http.GetAsync("reserva");
        if (!resposta.IsSuccessStatusCode) return new();
        return await resposta.Content.ReadFromJsonAsync<List<ReservaModel>>(JsonOpts) ?? new();
    }

    public async Task<ReservaModel?> ObterReservaAsync(int id)
    {
        await PrepararAuthAsync();
        var resposta = await _http.GetAsync($"reserva/{id}");
        if (!resposta.IsSuccessStatusCode) return null;
        return await resposta.Content.ReadFromJsonAsync<ReservaModel>(JsonOpts);
    }

    public async Task<(bool Sucesso, string Mensagem)> CancelarReservaAsync(int id)
    {
        await PrepararAuthAsync();
        var resposta = await _http.PutAsync($"reserva/{id}/cancelar", null);
        var corpo = await resposta.Content.ReadAsStringAsync();

        return resposta.IsSuccessStatusCode
            ? (true, "Reserva cancelada.")
            : (false, "Não foi possível cancelar a reserva.");
    }
}