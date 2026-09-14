using AgenciaViagens.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AgenciaViagens.Infrastructure.Services;

public class PdfService
{
    // Cores da marca
    private const string Violeta = "#7B2D8E";
    private const string VioletaForte = "#5C1F6B";
    private const string Laranja = "#F58220";
    private const string Pedra = "#F8F7F9";
    private const string Cinza = "#6E6873";
    private const string Linha = "#E4E0E8";

    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ══ VOUCHER DE RESERVA ═══════════════════════════════

    public byte[] GerarVoucher(Reserva reserva, string nomeCliente, string emailCliente)
    {
        var pacote = reserva.Pacote;
        var dias = (pacote.DataRegresso - pacote.DataPartida).Days;
        var titular = reserva.Participantes.FirstOrDefault(p => p.ETitular);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(t => t.FontFamily("Helvetica").FontSize(10).FontColor("#1F1A22"));

                // ── Cabeçalho ──
                page.Header().Background(Violeta).Padding(28).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("AGENCIA").FontSize(19).Bold().FontColor(Laranja);
                            t.Span("VIAGENS").FontSize(19).Bold().FontColor(Colors.White);
                            t.Span(".PT").FontSize(11).Bold().FontColor(Laranja);
                        });
                        col.Item().PaddingTop(2).Text("Agência de Viagens")
                            .FontSize(8).FontColor("#DCC8E4");
                    });

                    row.ConstantItem(170).AlignRight().Column(col =>
                    {
                        col.Item().Text("VOUCHER DE RESERVA")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        col.Item().PaddingTop(3).Text($"Nº {reserva.Id:D5}")
                            .FontSize(17).Bold().FontColor(Laranja);
                        col.Item().Text(reserva.Estado.ToUpper())
                            .FontSize(8).FontColor("#DCC8E4");
                    });
                });

                // ── Conteúdo ──
                page.Content().PaddingHorizontal(28).PaddingVertical(24).Column(col =>
                {
                    col.Spacing(20);

                    // Título da viagem
                    col.Item().Column(c =>
                    {
                        c.Item().Text($"{pacote.Origem}  →  {pacote.Destino}, {pacote.Pais}")
                            .FontSize(9).FontColor(Cinza);
                        c.Item().PaddingTop(3).Text(pacote.Nome)
                            .FontSize(20).Bold().FontColor(VioletaForte);
                    });

                    // Grelha de factos
                    col.Item().Border(1).BorderColor(Linha).Row(row =>
                    {
                        void Facto(IContainer c, string rotulo, string valor, bool ultimo = false)
                        {
                            var cell = c.Padding(13);
                            if (!ultimo) cell = cell.BorderRight(1).BorderColor(Linha);

                            cell.Column(x =>
                            {
                                x.Item().Text(rotulo).FontSize(7).FontColor(Cinza);
                                x.Item().PaddingTop(2).Text(valor).FontSize(11).Bold().FontColor(Violeta);
                            });
                        }

                        Facto(row.RelativeItem(), "PARTIDA", pacote.DataPartida.ToString("dd/MM/yyyy"));
                        Facto(row.RelativeItem(), "REGRESSO", pacote.DataRegresso.ToString("dd/MM/yyyy"));
                        Facto(row.RelativeItem(), "DURAÇÃO", $"{dias} dias");
                        Facto(row.RelativeItem(), "VIAJANTES", reserva.NumParticipantes.ToString(), true);
                    });

                    // Titular
                    col.Item().Column(c =>
                    {
                        c.Item().PaddingBottom(7).Text("TITULAR DA RESERVA")
                            .FontSize(8).Bold().FontColor(Violeta);

                        c.Item().Background(Pedra).Padding(13).Row(r =>
                        {
                            r.RelativeItem().Column(x =>
                            {
                                x.Item().Text(titular?.Nome ?? nomeCliente).FontSize(12).Bold();
                                x.Item().PaddingTop(2).Text(emailCliente).FontSize(9).FontColor(Cinza);
                            });
                            if (titular?.Documento is not null)
                            {
                                r.ConstantItem(150).AlignRight().Column(x =>
                                {
                                    x.Item().Text(titular.TipoDocumento ?? "Documento")
                                        .FontSize(7).FontColor(Cinza);
                                    x.Item().Text(titular.Documento).FontSize(11).Bold();
                                });
                            }
                        });
                    });

                    // Participantes
                    if (reserva.Participantes.Any())
                    {
                        col.Item().Column(c =>
                        {
                            c.Item().PaddingBottom(7).Text("QUEM VIAJA")
                                .FontSize(8).Bold().FontColor(Violeta);

                            c.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cd =>
                                {
                                    cd.RelativeColumn(3);
                                    cd.RelativeColumn(2);
                                    cd.RelativeColumn(2);
                                });

                                table.Header(h =>
                                {
                                    void Th(string texto) => h.Cell()
                                        .BorderBottom(1).BorderColor(Violeta)
                                        .PaddingBottom(5)
                                        .Text(texto).FontSize(7).Bold().FontColor(Cinza);

                                    Th("NOME");
                                    Th("DOCUMENTO");
                                    Th("NACIONALIDADE");
                                });

                                foreach (var p in reserva.Participantes)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Linha).PaddingVertical(8)
                                        .Text(t =>
                                        {
                                            t.Span(p.Nome).FontSize(10);
                                            if (p.ETitular)
                                                t.Span("  · titular").FontSize(8).FontColor(Laranja);
                                        });

                                    table.Cell().BorderBottom(1).BorderColor(Linha).PaddingVertical(8)
                                        .Text($"{p.TipoDocumento} {p.Documento}").FontSize(10);

                                    table.Cell().BorderBottom(1).BorderColor(Linha).PaddingVertical(8)
                                        .Text(p.Nacionalidade ?? "—").FontSize(10);
                                }
                            });
                        });
                    }

                    // Alojamento e total
                    col.Item().Border(1).BorderColor(Linha).Column(c =>
                    {
                        void Linha1(string rotulo, string valor, bool destaque = false)
                        {
                            c.Item().BorderBottom(destaque ? 0 : 1).BorderColor(Linha)
                                .Background(destaque ? Pedra : Colors.White)
                                .PaddingVertical(11).PaddingHorizontal(14)
                                .Row(r =>
                                {
                                    r.RelativeItem().Text(rotulo)
                                        .FontSize(destaque ? 10 : 9).FontColor(destaque ? "#1F1A22" : Cinza);
                                    r.ConstantItem(150).AlignRight().Text(valor)
                                        .FontSize(destaque ? 15 : 10)
                                        .Bold()
                                        .FontColor(destaque ? Violeta : "#1F1A22");
                                });
                        }

                        Linha1("Alojamento", reserva.OpcaoAlojamento ?? "Standard");
                        Linha1("Preço por pessoa",
                            (reserva.PrecoTotal / reserva.NumParticipantes).ToString("C"));
                        Linha1("Pontos ganhos", $"+{reserva.PontosGanhos}");
                        Linha1("TOTAL", reserva.PrecoTotal.ToString("C"), true);
                    });

                    // Observações
                    if (!string.IsNullOrWhiteSpace(reserva.Observacoes))
                    {
                        col.Item().Column(c =>
                        {
                            c.Item().PaddingBottom(5).Text("NOTA DO CLIENTE")
                                .FontSize(8).Bold().FontColor(Violeta);
                            c.Item().Text(reserva.Observacoes).FontSize(9).FontColor(Cinza);
                        });
                    }

                    // Condições
                    col.Item().BorderTop(1).BorderColor(Linha).PaddingTop(14).Column(c =>
                    {
                        c.Item().PaddingBottom(5).Text("CONDIÇÕES DE CANCELAMENTO")
                            .FontSize(8).Bold().FontColor(Violeta);
                        c.Item().Text("Devolução total até 30 dias antes da partida · 75% até 15 dias · 50% até 7 dias · sem reembolso nos últimos 7 dias.")
                            .FontSize(8).FontColor(Cinza).LineHeight(1.4f);
                    });
                });

                // ── Rodapé ──
                page.Footer().BorderTop(1).BorderColor(Linha).PaddingTop(11).PaddingHorizontal(28)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("AgenciaViagens · RNAVT 1234")
                                .FontSize(7).FontColor(Cinza);
                            c.Item().Text("Rua das Portas de Santo Antão 47, 1150-264 Lisboa · 213 456 789")
                                .FontSize(7).FontColor(Cinza);
                        });

                        row.ConstantItem(160).AlignRight().Text(t =>
                        {
                            t.Span($"Emitido a {DateTime.Now:dd/MM/yyyy}  ·  ")
                                .FontSize(7).FontColor(Cinza);
                            t.CurrentPageNumber().FontSize(7).FontColor(Cinza);
                            t.Span("/").FontSize(7).FontColor(Cinza);
                            t.TotalPages().FontSize(7).FontColor(Cinza);
                        });
                    });
            });
        }).GeneratePdf();
    }

    // ══ ITINERÁRIO DO PACOTE ═════════════════════════════

    public byte[] GerarItinerario(Pacote pacote)
    {
        var dias = (pacote.DataRegresso - pacote.DataPartida).Days;
        var itinerarios = pacote.Itinerarios.OrderBy(i => i.Dia).ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.DefaultTextStyle(t => t.FontFamily("Helvetica").FontSize(10).FontColor("#1F1A22"));

                page.Header().Background(Violeta).Padding(28).Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("AGENCIA").FontSize(15).Bold().FontColor(Laranja);
                        t.Span("VIAGENS").FontSize(15).Bold().FontColor(Colors.White);
                        t.Span(".PT").FontSize(9).Bold().FontColor(Laranja);
                    });

                    col.Item().PaddingTop(18).Text($"{pacote.Origem}  →  {pacote.Destino}, {pacote.Pais}")
                        .FontSize(9).FontColor("#DCC8E4");
                    col.Item().PaddingTop(3).Text(pacote.Nome)
                        .FontSize(23).Bold().FontColor(Colors.White);
                    col.Item().PaddingTop(6).Text($"{dias} dias · parte a {pacote.DataPartida:dd 'de' MMMM 'de' yyyy}")
                        .FontSize(10).FontColor("#DCC8E4");
                });

                page.Content().PaddingHorizontal(28).PaddingVertical(24).Column(col =>
                {
                    col.Spacing(18);

                    col.Item().Text(pacote.Descricao).FontSize(10).LineHeight(1.5f).FontColor(Cinza);

                    if (itinerarios.Any())
                    {
                        col.Item().PaddingTop(4).Text("DIA A DIA")
                            .FontSize(9).Bold().FontColor(Violeta);

                        foreach (var dia in itinerarios)
                        {
                            col.Item().Row(row =>
                            {
                                row.ConstantItem(58).Column(c =>
                                {
                                    c.Item().Background(Violeta).Padding(9).Column(x =>
                                    {
                                        x.Item().Text("DIA").FontSize(6).FontColor("#DCC8E4")
                                            .AlignCenter();
                                        x.Item().Text(dia.Dia.ToString()).FontSize(18).Bold()
                                            .FontColor(Colors.White).AlignCenter();
                                    });
                                });

                                row.RelativeItem().PaddingLeft(14).Column(c =>
                                {
                                    c.Item().Text(dia.Titulo).FontSize(12).Bold();
                                    c.Item().PaddingTop(3).Text(dia.Descricao)
                                        .FontSize(9).FontColor(Cinza).LineHeight(1.45f);

                                    if (!string.IsNullOrWhiteSpace(dia.Local))
                                    {
                                        c.Item().PaddingTop(3).Text(dia.Local)
                                            .FontSize(8).Bold().FontColor(Laranja);
                                    }
                                });
                            });
                        }
                    }
                    else
                    {
                        col.Item().Background(Pedra).Padding(22).AlignCenter()
                            .Text("O itinerário detalhado será enviado até 15 dias antes da partida.")
                            .FontSize(9).FontColor(Cinza);
                    }

                    col.Item().BorderTop(1).BorderColor(Linha).PaddingTop(16).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("A PARTIR DE").FontSize(7).FontColor(Cinza);
                            c.Item().PaddingTop(2)
                                .Text((pacote.PrecoPromocao ?? pacote.PrecoBase).ToString("C0"))
                                .FontSize(21).Bold().FontColor(Violeta);
                            c.Item().Text("por pessoa").FontSize(8).FontColor(Cinza);
                        });

                        row.ConstantItem(200).AlignRight().AlignBottom()
                            .Text("Reserve em agenciaviagens.pt")
                            .FontSize(9).FontColor(Cinza);
                    });
                });

                page.Footer().BorderTop(1).BorderColor(Linha).PaddingTop(11).PaddingHorizontal(28)
                    .Row(row =>
                    {
                        row.RelativeItem().Text("AgenciaViagens · RNAVT 1234 · 213 456 789")
                            .FontSize(7).FontColor(Cinza);

                        row.ConstantItem(70).AlignRight().Text(t =>
                        {
                            t.CurrentPageNumber().FontSize(7).FontColor(Cinza);
                            t.Span("/").FontSize(7).FontColor(Cinza);
                            t.TotalPages().FontSize(7).FontColor(Cinza);
                        });
                    });
            });
        }).GeneratePdf();
    }
}