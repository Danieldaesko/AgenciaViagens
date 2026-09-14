using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenciaViagens.Application.Services
{
    public class EstatisticasService
    {
        private readonly AppDbContext _context;

        public EstatisticasService(AppDbContext context) => _context = context;

        public async Task<ResumoDashboard> ObterResumoAsync()
        {
            var confirmadas = new[] { "Confirmada", "Concluida" };

            var totalReservas = await _context.Reservas.CountAsync();
            var reservasConfirmadas = await _context.Reservas
                .CountAsync(r => confirmadas.Contains(r.Estado));
            var reservasPendentes = await _context.Reservas
                .CountAsync(r => r.Estado == "Pendente");

            var faturacao = await _context.Reservas
                .Where(r => confirmadas.Contains(r.Estado))
                .SumAsync(r => (decimal?)r.PrecoTotal) ?? 0;

            var pacotesAtivos = await _context.Pacotes.CountAsync(p => p.Ativo);
            var clientes = await _context.Users.CountAsync();
            var avaliacoesPendentes = await _context.Avaliacoes.CountAsync(a => !a.Aprovada);

            var lugaresTotal = await _context.Pacotes
                .Where(p => p.Ativo)
                .SumAsync(p => (int?)p.VagasTotal) ?? 0;
            var lugaresOcupados = await _context.Pacotes
                .Where(p => p.Ativo)
                .SumAsync(p => (int?)p.VagasOcupadas) ?? 0;

            return new ResumoDashboard
            {
                TotalReservas = totalReservas,
                ReservasConfirmadas = reservasConfirmadas,
                ReservasPendentes = reservasPendentes,
                Faturacao = faturacao,
                TicketMedio = reservasConfirmadas > 0 ? faturacao / reservasConfirmadas : 0,
                PacotesAtivos = pacotesAtivos,
                Clientes = clientes,
                AvaliacoesPendentes = avaliacoesPendentes,
                TaxaOcupacao = lugaresTotal > 0 ? (double)lugaresOcupados / lugaresTotal * 100 : 0
            };
        }

        public async Task<List<PontoMensal>> ObterFaturacaoMensalAsync(int meses = 6)
        {
            var confirmadas = new[] { "Confirmada", "Concluida" };
            var desde = DateTime.UtcNow.AddMonths(-meses + 1);

            var dados = await _context.Reservas
                .Where(r => confirmadas.Contains(r.Estado) && r.DataReserva >= desde)
                .GroupBy(r => new { r.DataReserva.Year, r.DataReserva.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(r => r.PrecoTotal),
                    Contagem = g.Count()
                })
                .ToListAsync();

            var resultado = new List<PontoMensal>();

            for (var i = meses - 1; i >= 0; i--)
            {
                var data = DateTime.UtcNow.AddMonths(-i);
                var registo = dados.FirstOrDefault(d => d.Year == data.Year && d.Month == data.Month);

                resultado.Add(new PontoMensal
                {
                    Etiqueta = data.ToString("MMM yy"),
                    Valor = registo?.Total ?? 0,
                    Contagem = registo?.Contagem ?? 0
                });
            }

            return resultado;
        }

        public async Task<List<PontoCategoria>> ObterReservasPorCategoriaAsync()
        {
            var confirmadas = new[] { "Confirmada", "Concluida" };

            return await _context.Reservas
                .Where(r => confirmadas.Contains(r.Estado))
                .Include(r => r.Pacote)
                .GroupBy(r => r.Pacote.Categoria)
                .Select(g => new PontoCategoria
                {
                    Categoria = g.Key,
                    Reservas = g.Count(),
                    Faturacao = g.Sum(r => r.PrecoTotal)
                })
                .OrderByDescending(x => x.Reservas)
                .ToListAsync();
        }

        public async Task<List<PontoDestino>> ObterTopDestinosAsync(int limite = 5)
        {
            var confirmadas = new[] { "Confirmada", "Concluida" };

            return await _context.Reservas
                .Where(r => confirmadas.Contains(r.Estado))
                .Include(r => r.Pacote)
                .GroupBy(r => r.Pacote.Destino)
                .Select(g => new PontoDestino
                {
                    Destino = g.Key,
                    Viajantes = g.Sum(r => r.NumParticipantes),
                    Faturacao = g.Sum(r => r.PrecoTotal)
                })
                .OrderByDescending(x => x.Viajantes)
                .Take(limite)
                .ToListAsync();
        }
    }

    // ── Modelos de resultado ──────────────────────────

    public class ResumoDashboard
    {
        public int TotalReservas { get; set; }
        public int ReservasConfirmadas { get; set; }
        public int ReservasPendentes { get; set; }
        public decimal Faturacao { get; set; }
        public decimal TicketMedio { get; set; }
        public int PacotesAtivos { get; set; }
        public int Clientes { get; set; }
        public int AvaliacoesPendentes { get; set; }
        public double TaxaOcupacao { get; set; }
    }

    public class PontoMensal
    {
        public string Etiqueta { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int Contagem { get; set; }
    }

    public class PontoCategoria
    {
        public string Categoria { get; set; } = string.Empty;
        public int Reservas { get; set; }
        public decimal Faturacao { get; set; }
    }

    public class PontoDestino
    {
        public string Destino { get; set; } = string.Empty;
        public int Viajantes { get; set; }
        public decimal Faturacao { get; set; }
    }
}