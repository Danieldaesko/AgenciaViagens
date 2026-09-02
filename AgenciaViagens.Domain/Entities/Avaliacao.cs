using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Domain.Entities
{
    public class Avaliacao
    {
        public int Id { get; set; }
        public int UtilizadorId { get; set; }              // FK para AspNetUsers
        public int PacoteId { get; set; }
        public int Classificacao { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public bool Aprovada { get; set; } = false;
        public DateTime Data { get; set; } = DateTime.UtcNow;

        public Pacote Pacote { get; set; } = null!;
    }
}
