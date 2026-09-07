using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Application.DTOs;

namespace AgenciaViagens.Application.DTOs;

public class FiltroPacotesDTO
{
    public string? Destino { get; set; }
    public string? Pais { get; set; }
    public string Origem { get; set; } = "Lisboa";
    
    public int? Viajantes { get; set; }
    public string? Categoria { get; set; }
    public decimal? PrecoMin { get; set; }
    public decimal? PrecoMax { get; set; }
    public DateTime? DataPartidaDe { get; set; }
    public DateTime? DataPartidaAte { get; set; }
    public int? DuracaoMinDias { get; set; }
    public int? DuracaoMaxDias { get; set; }
    public bool? EmDestaque { get; set; }
    public bool? EmPromocao { get; set; }
    public bool ApenasComVagas { get; set; } = true;
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 9;
}