using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Mobile.Models;

public class ItinerarioModel
{
    public int Id { get; set; }
    public int PacoteId { get; set; }
    public int Dia { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? Local { get; set; }

    public string DiaTexto => $"Dia {Dia}";
}