using AgenciaViagens.Domain.Entities;
using Microsoft.EntityFrameworkCore; // Garante este using
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AgenciaViagens.Infrastructure.Identity;


namespace AgenciaViagens.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Pacote> Pacotes => Set<Pacote>();
        public DbSet<Reserva> Reservas => Set<Reserva>();
        public DbSet<Itinerario> Itinerarios => Set<Itinerario>();
        public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
        public DbSet<Fatura> Faturas => Set<Fatura>();
        public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
        public DbSet<PontosTransacao> PontosTransacoes => Set<PontosTransacao>();
        public DbSet<Participante> Participantes => Set<Participante>();
        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);   // OBRIGATÓRIO — configura as tabelas do Identity

            // ── Precisão decimal ──────────────────────────────
            b.Entity<Pacote>().Property(p => p.PrecoBase).HasPrecision(10, 2);
            b.Entity<Pacote>().Property(p => p.PrecoPromocao).HasPrecision(10, 2);
            b.Entity<Reserva>().Property(r => r.PrecoTotal).HasPrecision(10, 2);
            b.Entity<Reserva>().Property(r => r.DescontoPontos).HasPrecision(10, 2);
            b.Entity<Pagamento>().Property(p => p.Valor).HasPrecision(10, 2);
            b.Entity<Fatura>().Property(f => f.ValorSemIva).HasPrecision(10, 2);
            b.Entity<Fatura>().Property(f => f.TaxaIva).HasPrecision(5, 2);
            b.Entity<Fatura>().Property(f => f.ValorIva).HasPrecision(10, 2);
            b.Entity<Fatura>().Property(f => f.ValorTotal).HasPrecision(10, 2);

            // ── Índices ───────────────────────────────────────
            b.Entity<Pacote>().HasIndex(p => p.Destino);
            b.Entity<Pacote>().HasIndex(p => p.Categoria);
            b.Entity<Fatura>().HasIndex(f => f.Numero).IsUnique();
            b.Entity<Avaliacao>().HasIndex(a => new { a.UtilizadorId, a.PacoteId }).IsUnique();
            b.Entity<Itinerario>().HasIndex(i => new { i.PacoteId, i.Dia }).IsUnique();

            // ── Relações: Utilizador (Identity) ───────────────
            b.Entity<Reserva>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UtilizadorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Avaliacao>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Avaliacoes)
                .HasForeignKey(a => a.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<PontosTransacao>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(t => t.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Relações: Domínio ─────────────────────────────
            b.Entity<Reserva>()
                .HasOne(r => r.Pacote)
                .WithMany(p => p.Reservas)
                .HasForeignKey(r => r.PacoteId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Itinerario>()
                .HasOne(i => i.Pacote)
                .WithMany(p => p.Itinerarios)
                .HasForeignKey(i => i.PacoteId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Pagamento>()
                .HasOne(p => p.Reserva)
                .WithMany(r => r.Pagamentos)
                .HasForeignKey(p => p.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Participante>()
                .HasOne(p => p.Reserva)
                .WithMany(r => r.Participantes)
                .HasForeignKey(p => p.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Fatura>()
                .HasOne(f => f.Reserva)
                .WithMany(r => r.Faturas)
                .HasForeignKey(f => f.ReservaId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<Avaliacao>()
                .HasOne(a => a.Pacote)
                .WithMany(p => p.Avaliacoes)
                .HasForeignKey(a => a.PacoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Nomes das tabelas do Identity em português ────
            b.Entity<ApplicationUser>().ToTable("Utilizadores");
            b.Entity<ApplicationRole>().ToTable("Perfis");
        }
    }
}