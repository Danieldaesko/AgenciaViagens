using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Application.DTOs;

namespace AgenciaViagens.Application.DTOs;

public class ResultadoPaginadoDTO<T>
{
    public List<T> Itens { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanhoPagina);
    public bool TemAnterior => Pagina > 1;
    public bool TemSeguinte => Pagina < TotalPaginas;
}