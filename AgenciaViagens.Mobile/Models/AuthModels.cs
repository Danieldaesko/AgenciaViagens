using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Mobile.Models;

public class LoginPedido
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResposta
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public string? Token { get; set; }
    public DateTime? Expiracao { get; set; }
    public int UtilizadorId { get; set; }
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
    public int Pontos { get; set; }
}
