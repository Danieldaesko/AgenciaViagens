using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Domain.Entities
{
    public class PontosTransacao
    {
        public int Id { get; set; }
        public int UtilizadorId { get; set; }
        public int? ReservaId { get; set; }
        public int Pontos { get; set; } // positivo = ganhou, negativo = usou
        public string Descricao { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.UtcNow;

        public Utilizador Utilizador { get; set; } = null!;
    }
}