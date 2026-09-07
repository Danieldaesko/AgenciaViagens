using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Mobile.Models;

public class PacoteModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecoBase { get; set; }
    public decimal? PrecoPromocao { get; set; }
    public DateTime DataPartida { get; set; }
    public DateTime DataRegresso { get; set; }
    public int VagasTotal { get; set; }
    public int VagasOcupadas { get; set; }
    public string? ImagemUrl { get; set; }
    public bool EmDestaque { get; set; }
    public bool EmPromocao { get; set; }
    public List<ItinerarioModel> Itinerarios { get; set; } = new();

    // Propriedades calculadas para o binding
    public decimal Preco => PrecoPromocao ?? PrecoBase;
    public int VagasDisponiveis => VagasTotal - VagasOcupadas;
    public int Dias => (DataRegresso - DataPartida).Days;
    public string PrecoFormatado => $"{Preco:C0}";
    public string Percurso => $"{Origem} → {Destino}";
    public string ResumoDatas => $"{Dias} dias · parte a {DataPartida:dd MMM}";
    public string TextoVagas => VagasDisponiveis <= 3
        ? $"Só {VagasDisponiveis} lugares"
        : $"{VagasDisponiveis} lugares";
}