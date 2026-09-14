using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Application.DTOs;


namespace AgenciaViagens.Application.DTOs;

public class CriarAvaliacaoDTO
{
   
    public int PacoteId { get; set; }
    public int Classificacao { get; set; }
    public string Comentario { get; set; } = string.Empty;
}
