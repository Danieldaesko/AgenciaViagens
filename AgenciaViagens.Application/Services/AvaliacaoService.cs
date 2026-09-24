using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenciaViagens.Application.Services
{
    public class AvaliacaoService
    {
        private readonly AppDbContext _context;

        public AvaliacaoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Avaliacao>> ObterTodasAsync()
        {
            return await _context.Avaliacoes.ToListAsync();
        }

        public async Task<Avaliacao?> ObterPorIdAsync(int id)
        {
            return await _context.Avaliacoes.FindAsync(id);
        }

        public async Task AdicionarAsync(Avaliacao avaliacao)
        {
            await _context.Avaliacoes.AddAsync(avaliacao);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Avaliacao>> ObterPendentesAsync()
        {
            return await _context.Avaliacoes
                .Include(a => a.Pacote)
                .Where(a => !a.Aprovada)
                .OrderByDescending(a => a.Data)
                .ToListAsync();
        }

        public async Task<(bool Sucesso, string? Erro)> AprovarAsync(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao is null)
                return (false, "Avaliação não encontrada.");

            avaliacao.Aprovada = true;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Sucesso, string? Erro)> RejeitarAsync(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao is null)
                return (false, "Avaliação não encontrada.");

            _context.Avaliacoes.Remove(avaliacao);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ══ AVALIAÇÃO PELO CLIENTE ═══════════════════════════

        public async Task<(bool Pode, string? Motivo)> PodeAvaliarAsync(int reservaId, int utilizadorId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Pacote)
                .FirstOrDefaultAsync(r => r.Id == reservaId && r.UtilizadorId == utilizadorId);

            if (reserva is null)
                return (false, "Reserva não encontrada.");

            if (reserva.Estado is not ("Confirmada" or "Concluida"))
                return (false, "Só pode avaliar viagens confirmadas.");

            if (reserva.Pacote.DataRegresso > DateTime.Now)
                return (false, $"Pode avaliar depois do regresso, a {reserva.Pacote.DataRegresso:dd/MM/yyyy}.");

            var jaAvaliou = await _context.Avaliacoes
                .AnyAsync(a => a.UtilizadorId == utilizadorId && a.PacoteId == reserva.PacoteId);

            if (jaAvaliou)
                return (false, "Já avaliou esta viagem.");

            return (true, null);
        }

        public async Task<(bool Sucesso, string? Erro)> AvaliarAsync(
            int reservaId, int utilizadorId, int classificacao, string comentario)
        {
            var (pode, motivo) = await PodeAvaliarAsync(reservaId, utilizadorId);
            if (!pode)
                return (false, motivo);

            if (classificacao < 1 || classificacao > 5)
                return (false, "Escolha entre 1 e 5 estrelas.");

            if (string.IsNullOrWhiteSpace(comentario) || comentario.Trim().Length < 10)
                return (false, "Escreva pelo menos 10 caracteres sobre a viagem.");

            var reserva = await _context.Reservas.FindAsync(reservaId);

            var avaliacao = new Avaliacao
            {
                UtilizadorId = utilizadorId,
                PacoteId = reserva!.PacoteId,
                Classificacao = classificacao,
                Comentario = comentario.Trim(),
                Aprovada = false,
                Data = DateTime.UtcNow
            };

            await _context.Avaliacoes.AddAsync(avaliacao);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<List<Avaliacao>> ObterDoUtilizadorAsync(int utilizadorId)
        {
            return await _context.Avaliacoes
                .Include(a => a.Pacote)
                .Where(a => a.UtilizadorId == utilizadorId)
                .OrderByDescending(a => a.Data)
                .ToListAsync();
        }
    }
}