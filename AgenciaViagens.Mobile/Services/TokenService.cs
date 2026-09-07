using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Mobile.Services;

public class TokenService
{
    private const string ChaveToken = "jwt_token";
    private const string ChaveNome = "utilizador_nome";
    private const string ChaveEmail = "utilizador_email";
    private const string ChaveId = "utilizador_id";

    public async Task GuardarSessaoAsync(string token, int id, string nome, string email)
    {
        await SecureStorage.SetAsync(ChaveToken, token);
        await SecureStorage.SetAsync(ChaveNome, nome);
        await SecureStorage.SetAsync(ChaveEmail, email);
        await SecureStorage.SetAsync(ChaveId, id.ToString());
    }

    public async Task<string?> ObterTokenAsync()
        => await SecureStorage.GetAsync(ChaveToken);

    public async Task<string?> ObterNomeAsync()
        => await SecureStorage.GetAsync(ChaveNome);

    public async Task<string?> ObterEmailAsync()
        => await SecureStorage.GetAsync(ChaveEmail);

    public async Task<bool> EstaAutenticadoAsync()
        => !string.IsNullOrEmpty(await ObterTokenAsync());

    public void TerminarSessao()
    {
        SecureStorage.Remove(ChaveToken);
        SecureStorage.Remove(ChaveNome);
        SecureStorage.Remove(ChaveEmail);
        SecureStorage.Remove(ChaveId);
    }
}
