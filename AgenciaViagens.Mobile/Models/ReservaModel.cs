using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Mobile.Models;

public class ReservaModel
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public int PacoteId { get; set; }
    public int NumParticipantes { get; set; }
    public decimal PrecoTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? OpcaoAlojamento { get; set; }
    public int PontosGanhos { get; set; }
    public DateTime DataReserva { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public PacoteModel? Pacote { get; set; }
    public List<ParticipanteModel> Participantes { get; set; } = new();

    public string Referencia => $"#{Id:D5}";
    public string TotalFormatado => $"{PrecoTotal:C}";
    public string ResumoPessoas => NumParticipantes == 1 ? "1 pessoa" : $"{NumParticipantes} pessoas";

    public Color CorEstado => Estado switch
    {
        "Confirmada" => Color.FromArgb("#1E7A3D"),
        "Pendente" => Color.FromArgb("#C87A12"),
        "Cancelada" => Color.FromArgb("#C0392B"),
        "Concluida" => Color.FromArgb("#6E6873"),
        _ => Color.FromArgb("#6E6873")
    };
}