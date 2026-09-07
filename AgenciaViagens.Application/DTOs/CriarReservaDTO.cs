using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Application.DTOs;


namespace AgenciaViagens.Application.DTOs;

public class CriarReservaDTO
{
    public int UtilizadorId { get; set; }
    public int PacoteId { get; set; }
    public int NumParticipantes { get; set; }
    public decimal PrecoTotal { get; set; }
    public decimal DescontoPontos { get; set; }
    public string Estado { get; set; } = "Pendente";
    public string? OpcaoAlojamento { get; set; }
    public int PontosGanhos { get; set; }
    public string? Observacoes { get; set; }
}