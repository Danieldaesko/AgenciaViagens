namespace AgenciaViagens.Domain.Entities
{
    public class Reserva
    {
        public int Id { get; set; }
        public int UtilizadorId { get; set; }
        public int PacoteId { get; set; }
        public int NumParticipantes { get; set; }
        public string OpcaoAlojamento { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public string Estado { get; set; } = "Pendente";
        public DateTime DataReserva { get; set; } = DateTime.Now;
    }
}