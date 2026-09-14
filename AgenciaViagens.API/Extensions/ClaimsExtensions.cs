using System.Security.Claims;

namespace AgenciaViagens.API.Extensions;

public static class ClaimsExtensions
{
    /// <summary>
    /// Devolve o Id do utilizador autenticado, a partir do token JWT.
    /// </summary>
    public static int ObterUtilizadorId(this ClaimsPrincipal user)
    {
        var valor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : 0;
    }

    public static bool EhStaff(this ClaimsPrincipal user)
        => user.IsInRole("Admin") || user.IsInRole("Colaborador");
}