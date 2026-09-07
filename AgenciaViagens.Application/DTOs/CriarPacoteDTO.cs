using AgenciaViagens.Application.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AgenciaViagens.Application.DTOs;

public class CriarPacoteDTO
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique a cidade de partida.")]
    public string Origem { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal PrecoBase { get; set; }
    public decimal? PrecoPromocao { get; set; }
    public DateTime DataPartida { get; set; }
    public DateTime DataRegresso { get; set; }
    public int VagasTotal { get; set; }
    public string? ImagemUrl { get; set; }
    public bool EmDestaque { get; set; }
    public bool EmPromocao { get; set; }
}