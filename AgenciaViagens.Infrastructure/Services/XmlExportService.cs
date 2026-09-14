using System.Text;
using System.Xml;
using AgenciaViagens.Domain.Entities;

namespace AgenciaViagens.Infrastructure.Services;

public class XmlExportService
{
    private static readonly XmlWriterSettings Settings = new()
    {
        Indent = true,
        IndentChars = "  ",
        Encoding = new UTF8Encoding(false),
        OmitXmlDeclaration = false
    };

    // ══ RESERVAS ═════════════════════════════════════════

    public byte[] ExportarReservas(List<Reserva> reservas)
    {
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, Settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("Reservas");
            writer.WriteAttributeString("dataExportacao", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
            writer.WriteAttributeString("total", reservas.Count.ToString());

            foreach (var r in reservas)
            {
                writer.WriteStartElement("Reserva");
                writer.WriteAttributeString("id", r.Id.ToString());
                writer.WriteAttributeString("estado", r.Estado);

                writer.WriteElementString("DataReserva", r.DataReserva.ToString("yyyy-MM-dd"));
                writer.WriteElementString("NumParticipantes", r.NumParticipantes.ToString());
                writer.WriteElementString("OpcaoAlojamento", r.OpcaoAlojamento ?? "");
                writer.WriteElementString("PrecoTotal", r.PrecoTotal.ToString("F2"));
                writer.WriteElementString("PontosGanhos", r.PontosGanhos.ToString());

                if (r.DataCancelamento.HasValue)
                    writer.WriteElementString("DataCancelamento",
                        r.DataCancelamento.Value.ToString("yyyy-MM-dd"));

                // Pacote
                if (r.Pacote is not null)
                {
                    writer.WriteStartElement("Pacote");
                    writer.WriteAttributeString("id", r.Pacote.Id.ToString());
                    writer.WriteElementString("Nome", r.Pacote.Nome);
                    writer.WriteElementString("Origem", r.Pacote.Origem);
                    writer.WriteElementString("Destino", r.Pacote.Destino);
                    writer.WriteElementString("Pais", r.Pacote.Pais);
                    writer.WriteElementString("Categoria", r.Pacote.Categoria);
                    writer.WriteElementString("DataPartida", r.Pacote.DataPartida.ToString("yyyy-MM-dd"));
                    writer.WriteElementString("DataRegresso", r.Pacote.DataRegresso.ToString("yyyy-MM-dd"));
                    writer.WriteEndElement();
                }

                // Participantes
                if (r.Participantes.Any())
                {
                    writer.WriteStartElement("Participantes");
                    foreach (var p in r.Participantes)
                    {
                        writer.WriteStartElement("Participante");
                        writer.WriteAttributeString("titular", p.ETitular.ToString().ToLower());
                        writer.WriteElementString("Nome", p.Nome);
                        writer.WriteElementString("TipoDocumento", p.TipoDocumento ?? "");
                        writer.WriteElementString("Documento", p.Documento ?? "");
                        writer.WriteElementString("Nacionalidade", p.Nacionalidade ?? "");
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                // Pagamentos
                if (r.Pagamentos.Any())
                {
                    writer.WriteStartElement("Pagamentos");
                    foreach (var pg in r.Pagamentos)
                    {
                        writer.WriteStartElement("Pagamento");
                        writer.WriteAttributeString("estado", pg.Estado);
                        writer.WriteElementString("Data", pg.Data.ToString("yyyy-MM-dd"));
                        writer.WriteElementString("Metodo", pg.Metodo);
                        writer.WriteElementString("Valor", pg.Valor.ToString("F2"));
                        if (!string.IsNullOrWhiteSpace(pg.Referencia))
                            writer.WriteElementString("Referencia", pg.Referencia);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); // Reserva
            }

            writer.WriteEndElement(); // Reservas
            writer.WriteEndDocument();
        }

        return stream.ToArray();
    }

    // ══ PACOTES ══════════════════════════════════════════

    public byte[] ExportarPacotes(List<Pacote> pacotes)
    {
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, Settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("Pacotes");
            writer.WriteAttributeString("dataExportacao", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
            writer.WriteAttributeString("total", pacotes.Count.ToString());

            foreach (var p in pacotes)
            {
                writer.WriteStartElement("Pacote");
                writer.WriteAttributeString("id", p.Id.ToString());
                writer.WriteAttributeString("ativo", p.Ativo.ToString().ToLower());

                writer.WriteElementString("Nome", p.Nome);
                writer.WriteElementString("Descricao", p.Descricao);
                writer.WriteElementString("Origem", p.Origem);
                writer.WriteElementString("Destino", p.Destino);
                writer.WriteElementString("Pais", p.Pais);
                writer.WriteElementString("Categoria", p.Categoria);
                writer.WriteElementString("PrecoBase", p.PrecoBase.ToString("F2"));

                if (p.PrecoPromocao.HasValue)
                    writer.WriteElementString("PrecoPromocao", p.PrecoPromocao.Value.ToString("F2"));

                writer.WriteElementString("DataPartida", p.DataPartida.ToString("yyyy-MM-dd"));
                writer.WriteElementString("DataRegresso", p.DataRegresso.ToString("yyyy-MM-dd"));
                writer.WriteElementString("VagasTotal", p.VagasTotal.ToString());
                writer.WriteElementString("VagasOcupadas", p.VagasOcupadas.ToString());

                if (p.Itinerarios.Any())
                {
                    writer.WriteStartElement("Itinerario");
                    foreach (var i in p.Itinerarios.OrderBy(x => x.Dia))
                    {
                        writer.WriteStartElement("Dia");
                        writer.WriteAttributeString("numero", i.Dia.ToString());
                        writer.WriteElementString("Titulo", i.Titulo);
                        writer.WriteElementString("Descricao", i.Descricao);
                        if (!string.IsNullOrWhiteSpace(i.Local))
                            writer.WriteElementString("Local", i.Local);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); // Pacote
            }

            writer.WriteEndElement(); // Pacotes
            writer.WriteEndDocument();
        }

        return stream.ToArray();
    }
}
