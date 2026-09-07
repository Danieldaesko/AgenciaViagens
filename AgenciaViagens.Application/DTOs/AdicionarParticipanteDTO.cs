using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Application.DTOs;


namespace AgenciaViagens.Application.DTOs;

public class AdicionarParticipanteDTO
{
    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? TipoDocumento { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Nacionalidade { get; set; }
    public bool ETitular { get; set; }
}