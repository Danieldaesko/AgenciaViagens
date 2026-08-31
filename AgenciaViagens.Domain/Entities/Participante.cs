using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Domain.Entities
{
    public class Participante
    {
        public int Id { get; set; }
        public int ReservaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public Reserva? Reserva { get; set; }
    }
}