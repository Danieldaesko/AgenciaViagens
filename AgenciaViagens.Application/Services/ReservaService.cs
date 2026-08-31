using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenciaViagens.Application.Services
{
    public class ReservaService
    {
        private readonly AppDbContext _context;

        public ReservaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Reserva>> ObterTodasAsync()
        {
            return await _context.Reservas.ToListAsync();
        }

        public async Task<Reserva?> ObterPorIdAsync(int id)
        {
            return await _context.Reservas.FindAsync(id);
        }

        public async Task AdicionarAsync(Reserva reserva)
        {
            await _context.Reservas.AddAsync(reserva);
            await _context.SaveChangesAsync();
        }
    }
}