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
    public class PagamentoService
    {
        private readonly AppDbContext _context;

        public PagamentoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pagamento>> ObterPorReservaAsync(int reservaId)
        {
            return await _context.Pagamentos
                .Where(p => p.ReservaId == reservaId)
                .OrderBy(p => p.Data)
                .ToListAsync();
        }

        public async Task<(bool Sucesso, string? Erro, Pagamento? Pagamento)> RegistarPagamentoAsync(
            int reservaId, decimal valor, string metodo, string? referencia)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Pagamentos)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva is null)
                return (false, "Reserva não encontrada.", null);

            if (reserva.Estado is "Cancelada" or "Concluida")
                return (false, $"Não é possível pagar uma reserva {reserva.Estado.ToLower()}.", null);

            if (valor <= 0)
                return (false, "O valor do pagamento tem de ser positivo.", null);

            var jaPago = reserva.Pagamentos
                .Where(p => p.Estado == "Pago")
                .Sum(p => p.Valor);

            var emFalta = reserva.PrecoTotal - jaPago;

            if (valor > emFalta)
                return (false, $"O valor excede o montante em falta ({emFalta:C}).", null);

            var pagamento = new Pagamento
            {
                ReservaId = reservaId,
                Valor = valor,
                Metodo = metodo,
                Estado = "Pago",
                Referencia = referencia,
                Data = DateTime.UtcNow
            };

            await _context.Pagamentos.AddAsync(pagamento);
            await _context.SaveChangesAsync();

            return (true, null, pagamento);
        }

        public async Task<(decimal Total, decimal Pago, decimal EmFalta, bool Liquidado)> ObterEstadoPagamentoAsync(int reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Pagamentos)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva is null)
                return (0, 0, 0, false);

            var pago = reserva.Pagamentos
                .Where(p => p.Estado == "Pago")
                .Sum(p => p.Valor);

            var emFalta = reserva.PrecoTotal - pago;

            return (reserva.PrecoTotal, pago, emFalta, emFalta <= 0);
        }
    }
}