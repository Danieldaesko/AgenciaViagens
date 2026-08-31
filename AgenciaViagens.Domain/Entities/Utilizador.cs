using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgenciaViagens.Domain.Entities
{
    public class Utilizador
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? NIF { get; set; }
        public string? Telefone { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string Tipo { get; set; } = "Cliente"; // Cliente, Premium, Admin
        public int Pontos { get; set; } = 0;
        public bool ContaAtiva { get; set; } = false;
        public string? TokenAtivacao { get; set; }
        public string? TokenRecuperacao { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? UltimoLogin { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
        public ICollection<PontosTransacao> Transacoes { get; set; } = new List<PontosTransacao>();
    }
}
