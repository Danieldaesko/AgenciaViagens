using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Domain.Entities;

public static class ValidadorCartao
{
    public static string Limpar(string? numero)
        => new string((numero ?? "").Where(char.IsDigit).ToArray());

    /// <summary>Valida o número pelo algoritmo de Luhn (módulo 10).</summary>
    public static bool NumeroValido(string? numero)
    {
        var digitos = Limpar(numero);
        if (digitos.Length < 13 || digitos.Length > 19)
            return false;

        var soma = 0;
        var duplicar = false;

        for (var i = digitos.Length - 1; i >= 0; i--)
        {
            var n = digitos[i] - '0';
            if (duplicar)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            soma += n;
            duplicar = !duplicar;
        }

        return soma % 10 == 0;
    }

    /// <summary>Formato MM/AA e ainda dentro do prazo.</summary>
    public static bool ValidadeValida(string? validade)
    {
        if (string.IsNullOrWhiteSpace(validade))
            return false;

        var partes = validade.Trim().Split('/');
        if (partes.Length != 2
            || !int.TryParse(partes[0], out var mes)
            || !int.TryParse(partes[1], out var ano))
            return false;

        if (mes < 1 || mes > 12)
            return false;

        if (ano < 100) ano += 2000;

        // O cartão é válido até ao último dia do mês indicado
        var fimDoMes = new DateTime(ano, mes, 1).AddMonths(1).AddDays(-1);
        return fimDoMes >= DateTime.Today;
    }

    public static bool CvvValido(string? cvv, string numero)
    {
        var digitos = Limpar(cvv);
        var esperado = Bandeira(numero) == "American Express" ? 4 : 3;
        return digitos.Length == esperado;
    }

    public static string Bandeira(string? numero)
    {
        var d = Limpar(numero);
        if (d.StartsWith("4")) return "Visa";
        if (d.StartsWith("34") || d.StartsWith("37")) return "American Express";

        if (d.Length >= 2 && int.TryParse(d[..2], out var dois) && dois is >= 51 and <= 55)
            return "Mastercard";
        if (d.Length >= 4 && int.TryParse(d[..4], out var quatro) && quatro is >= 2221 and <= 2720)
            return "Mastercard";

        return "Cartão";
    }

    public static string Mascarar(string? numero)
    {
        var d = Limpar(numero);
        return d.Length >= 4 ? $"•••• {d[^4..]}" : "••••";
    }
}