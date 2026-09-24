using System.Globalization;
using System.Net;
using AgenciaViagens.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AgenciaViagens.Infrastructure.Services;

public class EmailService
{
    private static readonly CultureInfo PT = new("pt-PT");

    private readonly EmailOptions _opcoes;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailOptions> opcoes, ILogger<EmailService> logger)
    {
        _opcoes = opcoes.Value;
        _logger = logger;
    }

    // ══ ENVIO ════════════════════════════════════════════

    /// <summary>
    /// Envia um email. Nunca lança exceção: se o servidor falhar,
    /// regista o erro e devolve false, para não interromper a reserva.
    /// </summary>
    public async Task<bool> EnviarAsync(
        string para, string nome, string assunto, string corpoHtml,
        byte[]? anexo = null, string? nomeAnexo = null)
    {
        if (!_opcoes.Ativo)
        {
            _logger.LogInformation("Email desativado. Não enviado: {Assunto} para {Para}", assunto, para);
            return false;
        }

        try
        {
            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress(_opcoes.NomeRemetente, _opcoes.Remetente));
            mensagem.To.Add(new MailboxAddress(nome, para));
            mensagem.Subject = assunto;

            var corpo = new BodyBuilder { HtmlBody = Moldura(corpoHtml) };

            if (anexo is not null && nomeAnexo is not null)
                corpo.Attachments.Add(nomeAnexo, anexo, new ContentType("application", "pdf"));

            mensagem.Body = corpo.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_opcoes.Host, _opcoes.Porta,
                _opcoes.UsarSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            if (!string.IsNullOrWhiteSpace(_opcoes.Utilizador))
                await smtp.AuthenticateAsync(_opcoes.Utilizador, _opcoes.Password);

            await smtp.SendAsync(mensagem);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email enviado: {Assunto} para {Para}", assunto, para);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar email {Assunto} para {Para}", assunto, para);
            return false;
        }
    }

    // ══ MENSAGENS ════════════════════════════════════════

    public Task<bool> EnviarBoasVindasAsync(string email, string nome)
    {
        var corpo = $"""
            <h2 style="margin:0 0 12px;color:#5C1F6B">Olá, {E(nome)}</h2>
            <p>A sua conta na AgenciaViagens está criada. A partir de agora pode reservar viagens,
               acompanhar os itinerários e descarregar os vouchers na sua área de cliente.</p>
            {Botao("Ver as viagens", $"{_opcoes.UrlSite}/Pacotes")}
            <p style="color:#6E6873;font-size:13px">Se não foi você que criou esta conta, ignore esta mensagem.</p>
            """;

        return EnviarAsync(email, nome, "Bem-vindo à AgenciaViagens", corpo);
    }

    public Task<bool> EnviarAtivacaoAsync(string email, string nome, string linkAtivacao)
    {
        var corpo = $"""
            <h2 style="margin:0 0 12px;color:#5C1F6B">Confirme o seu email</h2>
            <p>Olá, {E(nome)}. Falta só um passo para ativar a sua conta na AgenciaViagens.
               Carregue no botão abaixo para confirmar que este email é seu.</p>
            {Botao("Ativar a minha conta", linkAtivacao)}
            <p style="color:#6E6873;font-size:13px">
               Se o botão não funcionar, copie este endereço para o navegador:<br>
               <span style="word-break:break-all">{E(linkAtivacao)}</span>
            </p>
            <p style="color:#6E6873;font-size:13px">Se não foi você que criou esta conta, ignore esta mensagem.</p>
            """;

        return EnviarAsync(email, nome, "Ative a sua conta AgenciaViagens", corpo);
    }

    public Task<bool> EnviarRecuperacaoAsync(string email, string nome, string linkRecuperacao)
    {
        var corpo = $"""
            <h2 style="margin:0 0 12px;color:#5C1F6B">Recuperar o acesso</h2>
            <p>Olá, {E(nome)}. Recebemos um pedido para redefinir a palavra-passe da sua conta.
               Carregue no botão para escolher uma nova. O link é válido por 1 hora.</p>
            {Botao("Definir nova palavra-passe", linkRecuperacao)}
            <p style="color:#6E6873;font-size:13px">
               Se o botão não funcionar, copie este endereço:<br>
               <span style="word-break:break-all">{E(linkRecuperacao)}</span>
            </p>
            <p style="color:#6E6873;font-size:13px">Se não pediu isto, ignore esta mensagem — a sua palavra-passe não muda.</p>
            """;

        return EnviarAsync(email, nome, "Recuperar o acesso à AgenciaViagens", corpo);
    }
    public Task<bool> EnviarReservaConfirmadaAsync(string email, string nome, Reserva reserva, byte[] voucherPdf)
    {
        var p = reserva.Pacote;

        var corpo = $"""
            <h2 style="margin:0 0 12px;color:#5C1F6B">Reserva confirmada</h2>
            <p>Olá, {E(nome)}. A sua reserva <strong>#{reserva.Id:D5}</strong> está confirmada.
               Segue em anexo o voucher com todos os detalhes.</p>
            {Tabela(
                ("Viagem", p.Nome),
                ("Percurso", $"{p.Origem} → {p.Destino}, {p.Pais}"),
                ("Partida", p.DataPartida.ToString("dd/MM/yyyy")),
                ("Regresso", p.DataRegresso.ToString("dd/MM/yyyy")),
                ("Viajantes", reserva.NumParticipantes.ToString()),
                ("Alojamento", reserva.OpcaoAlojamento ?? "Standard"),
                ("Total", reserva.PrecoTotal.ToString("C", PT)))}
            <p>Falta escolher a forma de pagamento. Pode pagar tudo ou só um sinal de 30%.</p>
            {Botao("Pagar a reserva", $"{_opcoes.UrlSite}/Pagamentos/Nova?reservaId={reserva.Id}")}
            """;

        return EnviarAsync(email, nome, $"Reserva #{reserva.Id:D5} confirmada", corpo,
            voucherPdf, $"voucher-{reserva.Id:D5}.pdf");
    }

    public Task<bool> EnviarPagamentoRecebidoAsync(
        string email, string nome, int reservaId, string pacote,
        decimal valor, string metodo, string? referencia, decimal emFalta)
    {
        var restante = emFalta > 0
            ? $"<p>Falta pagar <strong>{emFalta.ToString("C", PT)}</strong> até à data de partida.</p>"
            : "<p>A reserva está totalmente paga. A fatura fica disponível na sua área de cliente.</p>";

        var corpo = $"""
            <h2 style="margin:0 0 12px;color:#1E7A3D">Pagamento recebido</h2>
            <p>Olá, {E(nome)}. Recebemos o seu pagamento da reserva <strong>#{reservaId:D5}</strong>.</p>
            {Tabela(
                ("Viagem", pacote),
                ("Método", metodo),
                ("Referência", referencia ?? "—"),
                ("Valor", valor.ToString("C", PT)))}
            {restante}
            {Botao("Ver a reserva", $"{_opcoes.UrlSite}/Reservas/Detalhe/{reservaId}")}
            """;

        return EnviarAsync(email, nome, $"Pagamento recebido · Reserva #{reservaId:D5}", corpo);
    }

    public Task<bool> EnviarInstrucoesPagamentoAsync(
        string email, string nome, int reservaId, string titulo, string nota,
        params (string Rotulo, string Valor)[] dados)
    {
        var corpo = $"""
            <h2 style="margin:0 0 12px;color:#9A6208">{E(titulo)}</h2>
            <p>Olá, {E(nome)}. Para concluir o pagamento da reserva <strong>#{reservaId:D5}</strong>:</p>
            {Tabela(dados)}
            <p style="color:#6E6873;font-size:13px">{E(nota)}</p>
            {Botao("Ver a reserva", $"{_opcoes.UrlSite}/Reservas/Detalhe/{reservaId}")}
            """;

        return EnviarAsync(email, nome, $"{titulo} · Reserva #{reservaId:D5}", corpo);
    }

    // ══ AUXILIARES DE HTML ═══════════════════════════════

    /// <summary>Protege contra HTML injetado em nomes e textos do utilizador.</summary>
    private static string E(string? texto) => System.Net.WebUtility.HtmlEncode(texto ?? "");

    private static string Botao(string texto, string url) => $"""
        <p style="margin:24px 0">
          <a href="{url}" style="background:#F58220;color:#ffffff;text-decoration:none;
             padding:12px 26px;border-radius:30px;font-weight:bold;display:inline-block">{E(texto)}</a>
        </p>
        """;

    private static string Tabela(params (string Rotulo, string Valor)[] linhas)
    {
        var html = string.Join("", linhas.Select(l => $"""
            <tr>
              <td style="padding:10px 14px;border-bottom:1px solid #E4E0E8;color:#6E6873;font-size:14px">{E(l.Rotulo)}</td>
              <td style="padding:10px 14px;border-bottom:1px solid #E4E0E8;font-weight:bold;text-align:right;font-size:14px">{E(l.Valor)}</td>
            </tr>
            """));

        return $"""
            <table width="100%" cellpadding="0" cellspacing="0"
                   style="border:1px solid #E4E0E8;border-radius:8px;margin:18px 0;border-collapse:separate">
              {html}
            </table>
            """;
    }

    /// <summary>Cabeçalho e rodapé comuns. Estilos inline porque os clientes de email ignoram CSS externo.</summary>
    private static string Moldura(string corpo) => $"""
        <!DOCTYPE html>
        <html lang="pt">
        <body style="margin:0;padding:0;background:#F8F7F9;font-family:Arial,Helvetica,sans-serif">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#F8F7F9;padding:24px 0">
            <tr><td align="center">
              <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:10px;overflow:hidden">
                <tr><td style="background:#7B2D8E;padding:22px 28px">
                  <span style="font-size:20px;font-weight:bold;color:#F58220">AGENCIA</span><span style="font-size:20px;font-weight:bold;color:#ffffff">VIAGENS</span><span style="font-size:12px;font-weight:bold;color:#F58220">.PT</span>
                </td></tr>
                <tr><td style="padding:28px;color:#1F1A22;font-size:15px;line-height:1.6">
                  {corpo}
                </td></tr>
                <tr><td style="padding:18px 28px;border-top:1px solid #E4E0E8;color:#6E6873;font-size:12px">
                  AgenciaViagens · RNAVT 1234 · Rua das Portas de Santo Antão 47, 1150-264 Lisboa · 213 456 789
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}