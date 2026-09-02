namespace AgenciaViagens.Domain.Entities;

public class Participante
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? TipoDocumento { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Nacionalidade { get; set; }
    public bool ETitular { get; set; } = false;

    public Reserva Reserva { get; set; } = null!;
}