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
    public class PacoteService
    {
        private readonly AppDbContext _context;

        public PacoteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pacote>> ObterTodosAsync()
        {
            return await _context.Pacotes.ToListAsync();
        }

        public async Task<Pacote?> ObterPorIdAsync(int id)
        {
            return await _context.Pacotes.FindAsync(id);
        }

        public async Task AdicionarAsync(Pacote pacote)
        {
            await _context.Pacotes.AddAsync(pacote);
            await _context.SaveChangesAsync();
        }
    }
}