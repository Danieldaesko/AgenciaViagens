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
    public class ItinerarioService
    {
        private readonly AppDbContext _context;

        public ItinerarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Itinerario>> ObterTodosAsync()
        {
            return await _context.Itinerarios.ToListAsync();
        }

        public async Task<Itinerario?> ObterPorIdAsync(int id)
        {
            return await _context.Itinerarios.FindAsync(id);
        }

        public async Task AdicionarAsync(Itinerario itinerario)
        {
            await _context.Itinerarios.AddAsync(itinerario);
            await _context.SaveChangesAsync();
        }
    }
}