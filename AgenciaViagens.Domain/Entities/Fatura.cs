using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AgenciaViagens.Domain.Entities
{
    public class Fatura
    {
        public int Id { get; set; }
        public int ReservaId { get; set; }
        public string Numero { get; set; } = string.Empty;      // FAT-2026-0001
        public string Tipo { get; set; } = "Fatura";            // Fatura, Recibo, NotaCredito
        public string NomeCliente { get; set; } = string.Empty;
        public string? NifCliente { get; set; }
        public string? MoradaCliente { get; set; }
        public decimal ValorSemIva { get; set; }
        public decimal TaxaIva { get; set; } = 23m;
        public decimal ValorIva { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
        public bool Anulada { get; set; } = false;

        public Reserva Reserva { get; set; } = null!;
    }
}