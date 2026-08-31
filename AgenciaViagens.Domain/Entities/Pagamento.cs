using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Domain.Entities
{
    public class Pagamento
    {
        public int Id { get; set; }
        public int ReservaId { get; set; }
        public decimal Valor { get; set; }
        public string Metodo { get; set; } = string.Empty; // MB, Cartao, Transferencia
        public string Estado { get; set; } = "Pendente"; // Pendente, Pago, Reembolsado
        public string? Referencia { get; set; }
        public DateTime Data { get; set; } = DateTime.UtcNow;

        public Reserva Reserva { get; set; } = null!;
    }
}
