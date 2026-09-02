using AgenciaViagens.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AgenciaViagens.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string Nome { get; set; } = string.Empty;
        public string? NIF { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Morada { get; set; }
        public string? Preferencias { get; set; }
        public int Pontos { get; set; } = 0;
        public bool Ativo { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? UltimoLogin { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}