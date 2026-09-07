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
    public class FaturaService
    {
        private readonly AppDbContext _context;

        public FaturaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Fatura>> ObterPorReservaAsync(int reservaId)
        {
            return await _context.Faturas
                .Where(f => f.ReservaId == reservaId)
                .OrderBy(f => f.DataEmissao)
                .ToListAsync();
        }

        public async Task<Fatura?> ObterPorIdAsync(int id)
        {
            return await _context.Faturas
                .Include(f => f.Reserva)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<(bool Sucesso, string? Erro, Fatura? Fatura)> EmitirFaturaAsync(
            int reservaId, string tipo = "Fatura")
        {
            var reserva = await _context.Reservas
                .Include(r => r.Pacote)
                .Include(r => r.Participantes)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva is null)
                return (false, "Reserva não encontrada.", null);

            if (reserva.Estado == "Cancelada")
                return (false, "Não é possível faturar uma reserva cancelada.", null);

            // Dados do titular
            var titular = reserva.Participantes.FirstOrDefault(p => p.ETitular);
            var nomeCliente = titular?.Nome ?? "Cliente";

            // Numeração sequencial: FAT-2026-0001
            var ano = DateTime.UtcNow.Year;
            var prefixo = tipo == "Recibo" ? "REC" : "FAT";
            var ultimoNumero = await _context.Faturas
                .Where(f => f.Numero.StartsWith($"{prefixo}-{ano}-"))
                .CountAsync();

            var numero = $"{prefixo}-{ano}-{(ultimoNumero + 1):D4}";

            // Cálculo de IVA (23%)
            const decimal taxaIva = 23m;
            var valorTotal = reserva.PrecoTotal;
            var valorSemIva = Math.Round(valorTotal / (1 + taxaIva / 100), 2);
            var valorIva = valorTotal - valorSemIva;

            var fatura = new Fatura
            {
                ReservaId = reservaId,
                Numero = numero,
                Tipo = tipo,
                NomeCliente = nomeCliente,
                NifCliente = titular?.Documento,
                ValorSemIva = valorSemIva,
                TaxaIva = taxaIva,
                ValorIva = valorIva,
                ValorTotal = valorTotal,
                DataEmissao = DateTime.UtcNow,
                Anulada = false
            };

            await _context.Faturas.AddAsync(fatura);
            await _context.SaveChangesAsync();

            return (true, null, fatura);
        }

        public async Task<(bool Sucesso, string? Erro)> AnularFaturaAsync(int faturaId)
        {
            var fatura = await _context.Faturas.FindAsync(faturaId);

            if (fatura is null)
                return (false, "Fatura não encontrada.");

            if (fatura.Anulada)
                return (false, "A fatura já está anulada.");

            fatura.Anulada = true;
            await _context.SaveChangesAsync();

            return (true, null);
        }
    }
}
