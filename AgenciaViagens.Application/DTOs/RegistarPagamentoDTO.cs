using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Application.DTOs;

namespace AgenciaViagens.Application.DTOs;

public class RegistarPagamentoDTO
{
    public decimal Valor { get; set; }
    public string Metodo { get; set; } = string.Empty;   // MB, Cartao, Transferencia
    public string? Referencia { get; set; }
}