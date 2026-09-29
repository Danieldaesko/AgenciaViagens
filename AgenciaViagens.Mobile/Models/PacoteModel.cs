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

    // Imagem empacotada na app, escolhida pelo destino.
    // Permite mostrar fotos sem dependência de rede no dispositivo.
    public string ImagemLocal
    {
        get
        {
            var d = (Destino ?? string.Empty).Trim().ToLowerInvariant();

            if (d.Contains("roma")) return "roma.jpg";
            if (d.Contains("barcelona")) return "barcelona.jpg";
            if (d.Contains("luanda")) return "luanda.jpg";
            if (d.Contains("lubango") || d.Contains("huila")) return "huila.jpg";
            if (d.Contains("rio")) return "rio.jpg";

            return string.Empty;   // fica a cor de fundo do cartão
        }
    }

}