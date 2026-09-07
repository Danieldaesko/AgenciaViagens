using AgenciaViagens.Application.DTOs;
using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenciaViagens.Application.Services
{
    public class PacoteService
    {
        private readonly AppDbContext _context;

        public PacoteService(AppDbContext context)
        {
            _context = context;
        }

        // ══ MÉTODOS EXISTENTES ═══════════════════════════════

        public async Task<List<Pacote>> ObterTodosAsync()
        {
            return await _context.Pacotes.ToListAsync();
        }

        public async Task<Pacote?> ObterPorIdAsync(int id)
        {
            return await _context.Pacotes
                .Include(p => p.Itinerarios)
                .Include(p => p.Avaliacoes.Where(a => a.Aprovada))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AdicionarAsync(Pacote pacote)
        {
            await _context.Pacotes.AddAsync(pacote);
            await _context.SaveChangesAsync();
        }

        // ══ PESQUISA COM FILTROS ═════════════════════════════

        public async Task<ResultadoPaginadoDTO<Pacote>> PesquisarAsync(FiltroPacotesDTO filtro)
        {
            var query = _context.Pacotes
                .Where(p => p.Ativo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.Destino))
                query = query.Where(p => p.Destino.Contains(filtro.Destino));

            if (!string.IsNullOrWhiteSpace(filtro.Pais))
                query = query.Where(p => p.Pais.Contains(filtro.Pais));

            if (!string.IsNullOrWhiteSpace(filtro.Origem))
                query = query.Where(p => p.Origem == filtro.Origem);

            if (filtro.Viajantes.HasValue && filtro.Viajantes > 0)
                query = query.Where(p => p.VagasTotal - p.VagasOcupadas >= filtro.Viajantes.Value);

            if (!string.IsNullOrWhiteSpace(filtro.Categoria))
                query = query.Where(p => p.Categoria == filtro.Categoria);

            if (filtro.PrecoMin.HasValue)
                query = query.Where(p => (p.PrecoPromocao ?? p.PrecoBase) >= filtro.PrecoMin.Value);

            if (filtro.PrecoMax.HasValue)
                query = query.Where(p => (p.PrecoPromocao ?? p.PrecoBase) <= filtro.PrecoMax.Value);

            if (filtro.DataPartidaDe.HasValue)
                query = query.Where(p => p.DataPartida >= filtro.DataPartidaDe.Value);

            if (filtro.DataPartidaAte.HasValue)
                query = query.Where(p => p.DataPartida <= filtro.DataPartidaAte.Value);

            if (filtro.DuracaoMinDias.HasValue)
                query = query.Where(p => EF.Functions.DateDiffDay(p.DataPartida, p.DataRegresso) >= filtro.DuracaoMinDias.Value);

            if (filtro.DuracaoMaxDias.HasValue)
                query = query.Where(p => EF.Functions.DateDiffDay(p.DataPartida, p.DataRegresso) <= filtro.DuracaoMaxDias.Value);

            if (filtro.EmDestaque.HasValue)
                query = query.Where(p => p.EmDestaque == filtro.EmDestaque.Value);

            if (filtro.EmPromocao.HasValue)
                query = query.Where(p => p.EmPromocao == filtro.EmPromocao.Value);

            if (filtro.ApenasComVagas)
                query = query.Where(p => p.VagasOcupadas < p.VagasTotal);

            var total = await query.CountAsync();

            var itens = await query
                .OrderByDescending(p => p.EmDestaque)
                .ThenBy(p => p.DataPartida)
                .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
                .Take(filtro.TamanhoPagina)
                .ToListAsync();

            return new ResultadoPaginadoDTO<Pacote>
            {
                Itens = itens,
                Total = total,
                Pagina = filtro.Pagina,
                TamanhoPagina = filtro.TamanhoPagina
            };
        }

        // ══ LISTAS AUXILIARES ════════════════════════════════

        public async Task<List<Pacote>> ObterDestaquesAsync(int limite = 6)
        {
            return await _context.Pacotes
                .Where(p => p.Ativo && p.EmDestaque && p.VagasOcupadas < p.VagasTotal)
                .OrderBy(p => p.DataPartida)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<List<Pacote>> ObterPromocoesAsync(int limite = 6)
        {
            return await _context.Pacotes
                .Where(p => p.Ativo && p.EmPromocao && p.PrecoPromocao != null && p.VagasOcupadas < p.VagasTotal)
                .OrderBy(p => p.PrecoPromocao)
                .Take(limite)
                .ToListAsync();
        }

        public async Task<List<string>> ObterCategoriasAsync()
        {
            return await _context.Pacotes
                .Where(p => p.Ativo)
                .Select(p => p.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> ObterDestinosAsync()
        {
            return await _context.Pacotes
                .Where(p => p.Ativo)
                .Select(p => p.Destino)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        public async Task<List<string>> ObterOrigensAsync()
        {
            return await _context.Pacotes
                .Where(p => p.Ativo)
                .Select(p => p.Origem)
                .Distinct()
                .OrderBy(o => o)
                .ToListAsync();
        }
    }
}