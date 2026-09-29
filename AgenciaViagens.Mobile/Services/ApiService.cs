using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
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

    /// <summary>
    /// Traduz uma falha de rede numa mensagem útil durante o desenvolvimento.
    /// </summary>
    private static string DescreverFalha(Exception ex)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"[API] {ex}");
        return $"Falha de ligação: {ex.GetBaseException().Message}";
#else
        return "Não foi possível ligar ao servidor.";
#endif
    }

    private static string DescreverEstado(HttpStatusCode codigo) => codigo switch
    {
        HttpStatusCode.Unauthorized => "Sessão inválida ou expirada.",
        HttpStatusCode.Forbidden => "Não tem permissão para esta operação.",
        HttpStatusCode.NotFound => "Recurso não encontrado (verifique a rota).",
        _ => $"O servidor respondeu {(int)codigo} {codigo}."
    };

    // ── AUTENTICAÇÃO ──────────────────────────────────

    public async Task<LoginResposta> LoginAsync(string email, string password)
    {
        try
        {
            var resposta = await _http.PostAsJsonAsync("auth/login",
                new LoginPedido { Email = email, Password = password });

            if (!resposta.IsSuccessStatusCode)
            {
                var motivo = resposta.StatusCode == HttpStatusCode.Unauthorized
                    ? "Email ou palavra-passe incorretos."
                    : DescreverEstado(resposta.StatusCode);

                return new LoginResposta { Sucesso = false, Mensagem = motivo };
            }

            var resultado = await resposta.Content.ReadFromJsonAsync<LoginResposta>(JsonOpts);

            if (resultado?.Token is not null)
            {
                await _tokenService.GuardarSessaoAsync(
                    resultado.Token, resultado.UtilizadorId,
                    resultado.Nome ?? "", resultado.Email ?? "");
            }

            return resultado ?? new LoginResposta { Sucesso = false, Mensagem = "Resposta inválida." };
        }
        catch (Exception ex)
        {
            return new LoginResposta { Sucesso = false, Mensagem = DescreverFalha(ex) };
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
        var resposta = await _http.GetAsync("reserva/minhas");
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
        try
        {
            await PrepararAuthAsync();
            var resposta = await _http.PutAsync($"reserva/{id}/cancelar", null);

            if (resposta.IsSuccessStatusCode)
                return (true, "Reserva cancelada.");

            var corpo = await resposta.Content.ReadAsStringAsync();

            return (false, string.IsNullOrWhiteSpace(corpo)
                ? DescreverEstado(resposta.StatusCode)
                : corpo);
        }
        catch (Exception ex)
        {
            return (false, DescreverFalha(ex));
        }
    }
}