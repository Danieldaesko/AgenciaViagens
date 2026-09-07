using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Application.DTOs;


namespace AgenciaViagens.Application.DTOs;

public class CriarItinerarioDTO
{
    public int PacoteId { get; set; }
    public int Dia { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? Local { get; set; }
}