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

        // ══ MÉTODOS EXISTENTES ═══════════════════════════════

        public async Task<List<Reserva>> ObterTodasAsync()
        {
            return await _context.Reservas
                .Include(r => r.Pacote)
                .ToListAsync();
        }

        public async Task<Reserva?> ObterPorIdAsync(int id)
        {
            return await _context.Reservas
                .Include(r => r.Pacote)
                .Include(r => r.Participantes)
                .Include(r => r.Pagamentos)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AdicionarAsync(Reserva reserva)
        {
            await _context.Reservas.AddAsync(reserva);
            await _context.SaveChangesAsync();
        }

        // ══ PARTICIPANTES ════════════════════════════════════

        public async Task<(bool Sucesso, string? Erro)> AdicionarParticipanteAsync(
            int reservaId, Participante participante)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Participantes)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva is null)
                return (false, "Reserva não encontrada.");

            if (reserva.Estado is "Cancelada" or "Concluida")
                return (false, "Não é possível alterar uma reserva cancelada ou concluída.");

            if (reserva.Participantes.Count >= reserva.NumParticipantes)
                return (false, $"Limite atingido: a reserva é para {reserva.NumParticipantes} pessoa(s).");

            if (participante.ETitular && reserva.Participantes.Any(p => p.ETitular))
                return (false, "Já existe um titular nesta reserva.");

            participante.ReservaId = reservaId;
            await _context.Participantes.AddAsync(participante);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Valido, string? Erro)> ValidarParticipantesAsync(int reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Participantes)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva is null)
                return (false, "Reserva não encontrada.");

            var registados = reserva.Participantes.Count;
            var esperados = reserva.NumParticipantes;

            if (registados != esperados)
            {
                return (false, registados < esperados
                    ? $"Faltam dados de {esperados - registados} participante(s). Registados: {registados} de {esperados}."
                    : $"Há participantes a mais. Registados: {registados}, esperados: {esperados}.");
            }

            if (!reserva.Participantes.Any(p => p.ETitular))
                return (false, "É obrigatório indicar qual dos participantes é o titular.");

            if (reserva.Participantes.Count(p => p.ETitular) > 1)
                return (false, "Só pode existir um titular por reserva.");

            var semDocumento = reserva.Participantes
                .Where(p => string.IsNullOrWhiteSpace(p.Documento))
                .Select(p => p.Nome)
                .ToList();

            if (semDocumento.Any())
                return (false, $"Sem documento de identificação: {string.Join(", ", semDocumento)}.");

            return (true, null);
        }

        // ══ ESTADOS DA RESERVA ═══════════════════════════════

        public async Task<(bool Sucesso, string? Erro)> ConfirmarReservaAsync(int reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Participantes)
                .Include(r => r.Pacote)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva is null)
                return (false, "Reserva não encontrada.");

            if (reserva.Estado != "Pendente")
                return (false, $"Só é possível confirmar reservas pendentes. Estado atual: {reserva.Estado}.");

            var (valido, erro) = await ValidarParticipantesAsync(reservaId);
            if (!valido)
                return (false, erro);

            var disponiveis = reserva.Pacote.VagasTotal - reserva.Pacote.VagasOcupadas;
            if (disponiveis < reserva.NumParticipantes)
                return (false, $"Vagas insuficientes. Disponíveis: {disponiveis}.");

            reserva.Pacote.VagasOcupadas += reserva.NumParticipantes;
            reserva.Estado = "Confirmada";

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Sucesso, string? Erro, decimal Reembolso)> CancelarReservaAsync(int reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Pacote)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva is null)
                return (false, "Reserva não encontrada.", 0);

            if (reserva.Estado is "Cancelada" or "Concluida")
                return (false, $"A reserva já está {reserva.Estado.ToLower()}.", 0);

            // Política de reembolso por antecedência
            var dias = (reserva.Pacote.DataPartida - DateTime.UtcNow).Days;
            var percentagem = dias switch
            {
                >= 30 => 1.00m,
                >= 15 => 0.75m,
                >= 7 => 0.50m,
                _ => 0.00m
            };

            var reembolso = reserva.PrecoTotal * percentagem;

            if (reserva.Estado == "Confirmada")
                reserva.Pacote.VagasOcupadas -= reserva.NumParticipantes;

            reserva.Estado = "Cancelada";
            reserva.DataCancelamento = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, null, reembolso);
        }
    }
}