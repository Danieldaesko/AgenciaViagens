using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenciaViagens.Application.Services
{
    public class PagamentoService
    {
        private readonly AppDbContext _context;

        public const string EntidadeMultibanco = "21234";
        public const string IbanAgencia = "PT50 0035 0000 1234 5678 9015 4";

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

        public async Task<Pagamento?> ObterPorIdAsync(int id)
        {
            return await _context.Pagamentos
                .Include(p => p.Reserva)
                    .ThenInclude(r => r.Pacote)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Valor que ainda pode ser pago: total menos o que está pago ou pendente.
        /// Impede gerar duas referências Multibanco para o mesmo montante.
        /// </summary>
        public async Task<decimal> ObterValorDisponivelAsync(int reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Pagamentos)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva is null) return 0;

            var comprometido = reserva.Pagamentos
                .Where(p => p.Estado is "Pago" or "Pendente")
                .Sum(p => p.Valor);

            return Math.Max(0, reserva.PrecoTotal - comprometido);
        }

        public async Task<(bool Sucesso, string? Erro, Pagamento? Pagamento)> RegistarPagamentoAsync(
            int reservaId, decimal valor, string metodo, string? referencia, string estado = "Pago")
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

            var disponivel = await ObterValorDisponivelAsync(reservaId);
            if (valor > disponivel)
                return (false, $"O valor excede o montante em falta ({disponivel:C}).", null);

            var pagamento = new Pagamento
            {
                ReservaId = reservaId,
                Valor = valor,
                Metodo = metodo,
                Estado = estado,
                Referencia = referencia,
                Data = DateTime.UtcNow
            };

            await _context.Pagamentos.AddAsync(pagamento);
            await _context.SaveChangesAsync();

            return (true, null, pagamento);
        }

        /// <summary>Colaborador confirma que o Multibanco ou a transferência chegou.</summary>
        public async Task<(bool Sucesso, string? Erro, int ReservaId)> ConfirmarPagamentoAsync(int pagamentoId)
        {
            var pagamento = await _context.Pagamentos.FindAsync(pagamentoId);

            if (pagamento is null)
                return (false, "Pagamento não encontrado.", 0);

            if (pagamento.Estado != "Pendente")
                return (false, "Este pagamento já não está pendente.", pagamento.ReservaId);

            pagamento.Estado = "Pago";
            await _context.SaveChangesAsync();

            return (true, null, pagamento.ReservaId);
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

        /// <summary>
        /// Referência Multibanco de 9 dígitos, sempre igual para a mesma reserva e valor.
        /// Num sistema real seria gerada pelo gateway (SIBS, Easypay).
        /// </summary>
        public static string GerarReferenciaMultibanco(int reservaId, decimal valor)
        {
            var centimos = (int)(valor * 100) % 1000;
            var bruto = $"{reservaId % 1000000:D6}{centimos:D3}";
            return $"{bruto[..3]} {bruto[3..6]} {bruto[6..]}";
        }
    }
}