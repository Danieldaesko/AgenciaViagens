using AgenciaViagens.Domain.Entities;
using Microsoft.EntityFrameworkCore; // Garante este using

namespace AgenciaViagens.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Pacote> Pacotes { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Itinerario> Itinerarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reserva>()
                .Property(r => r.ValorTotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Pacote>()
                .Property(p => p.PrecoBase)
                .HasColumnType("decimal(18,2)");
        }
    }
}