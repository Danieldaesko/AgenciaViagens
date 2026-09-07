using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Mobile.Models;

public class ParticipanteModel
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? TipoDocumento { get; set; }
    public bool ETitular { get; set; }

    public string Papel => ETitular ? "Titular" : "Acompanhante";
}
