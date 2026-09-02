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
        public int UtilizadorId { get; set; }              // FK para AspNetUsers
        public int? ReservaId { get; set; }
        public int Pontos { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime Data { get; set; } = DateTime.UtcNow;
    }
}