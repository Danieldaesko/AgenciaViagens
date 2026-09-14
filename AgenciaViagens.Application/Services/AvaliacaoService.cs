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
    }
}