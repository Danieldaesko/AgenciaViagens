using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Domain.Entities
{
    public class Pacote
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty; // praia, cultura, aventura
        public decimal PrecoBase { get; set; }
        public decimal? PrecoPromocao { get; set; }
        public DateTime DataPartida { get; set; }
        public DateTime DataRegresso { get; set; }
        public int VagasTotal { get; set; }
        public int VagasOcupadas { get; set; } = 0;
        public string? ImagemUrl { get; set; }
        public bool EmDestaque { get; set; } = false;
        public bool EmPromocao { get; set; } = false;
        public bool Ativo { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public ICollection<Itinerario> Itinerarios { get; set; } = new List<Itinerario>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}
