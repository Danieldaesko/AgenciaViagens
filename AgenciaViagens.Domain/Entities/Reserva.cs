namespace AgenciaViagens.Domain.Entities
{
    public class Reserva
    {
        public int Id { get; set; }
        public int UtilizadorId { get; set; }              // FK para AspNetUsers
        public int PacoteId { get; set; }
        public int NumParticipantes { get; set; } = 1;
        public decimal PrecoTotal { get; set; }
        public decimal DescontoPontos { get; set; } = 0;
        public string Estado { get; set; } = "Pendente";
        public string? OpcaoAlojamento { get; set; }
        public int PontosGanhos { get; set; } = 0;
        public string? Observacoes { get; set; }
        public string? MotivoAlteracao { get; set; }
        public DateTime DataReserva { get; set; } = DateTime.UtcNow;
        public DateTime? DataCancelamento { get; set; }

        public Pacote Pacote { get; set; } = null!;
        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
        public ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();
        public ICollection<Participante> Participantes { get; set; } = new List<Participante>();
    }
}