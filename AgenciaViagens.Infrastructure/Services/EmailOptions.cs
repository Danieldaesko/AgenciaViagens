using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Infrastructure.Services;

public class EmailOptions
{
    public bool Ativo { get; set; } = true;
    public string Host { get; set; } = "localhost";
    public int Porta { get; set; } = 2525;
    public bool UsarSsl { get; set; } = false;
    public string? Utilizador { get; set; }
    public string? Password { get; set; }
    public string Remetente { get; set; } = "reservas@agenciaviagens.pt";
    public string NomeRemetente { get; set; } = "AgenciaViagens";
    public string UrlSite { get; set; } = "https://localhost:7273";
}