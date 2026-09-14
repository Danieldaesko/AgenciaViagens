using AgenciaViagens.API.Extensions;
using AgenciaViagens.Application.DTOs;
using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Identity;
using AgenciaViagens.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservaController : ControllerBase
    {
        private readonly ReservaService _reservaService;
        private readonly PdfService _pdfService;
        private readonly XmlExportService _xmlService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservaController(
            ReservaService reservaService,
            PdfService pdfService,
             XmlExportService xmlService,
            UserManager<ApplicationUser> userManager)
        {
            _reservaService = reservaService;
            _pdfService = pdfService;
            _xmlService = xmlService;
            _userManager = userManager;
        }

        /// <summary>Todas as reservas do sistema. Só staff.</summary>
        [HttpGet]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Colaborador}")]
        public async Task<ActionResult<List<Reserva>>> ObterTodas()
        {
            var reservas = await _reservaService.ObterTodasAsync();
            return Ok(reservas);
        }

        /// <summary>Reservas do utilizador autenticado.</summary>
        [HttpGet("minhas")]
        public async Task<ActionResult<List<Reserva>>> ObterMinhas()
        {
            var utilizadorId = User.ObterUtilizadorId();
            var reservas = await _reservaService.ObterPorUtilizadorAsync(utilizadorId);
            return Ok(reservas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> ObterPorId(int id)
        {
            var utilizadorId = User.ObterUtilizadorId();

            // Staff vê qualquer reserva; cliente só vê as suas
            var reserva = User.EhStaff()
                ? await _reservaService.ObterPorIdAsync(id)
                : await _reservaService.ObterDoUtilizadorAsync(id, utilizadorId);

            if (reserva is null)
                return NotFound(new { erro = "Reserva não encontrada." });

            return Ok(reserva);
        }

        [HttpPost]
        public async Task<ActionResult<Reserva>> Criar([FromBody] CriarReservaDTO dto)
        {
            if (dto.PacoteId <= 0)
                return BadRequest(new { erro = "PacoteId inválido." });

            if (dto.NumParticipantes < 1)
                return BadRequest(new { erro = "A reserva tem de ter pelo menos 1 participante." });

            var reserva = new Reserva
            {
                UtilizadorId = User.ObterUtilizadorId(),
                PacoteId = dto.PacoteId,
                NumParticipantes = dto.NumParticipantes,
                PrecoTotal = dto.PrecoTotal,
                DescontoPontos = dto.DescontoPontos,
                Estado = "Pendente",
                OpcaoAlojamento = dto.OpcaoAlojamento,
                PontosGanhos = dto.PontosGanhos,
                Observacoes = dto.Observacoes
            };

            await _reservaService.AdicionarAsync(reserva);
            return CreatedAtAction(nameof(ObterPorId), new { id = reserva.Id }, reserva);
        }

        [HttpPost("{id}/participantes")]
        public async Task<IActionResult> AdicionarParticipante(int id, [FromBody] AdicionarParticipanteDTO dto)
        {
            if (!await PodeAcederAsync(id))
                return Forbid();

            var participante = new Participante
            {
                ReservaId = id,
                Nome = dto.Nome,
                Documento = dto.Documento,
                TipoDocumento = dto.TipoDocumento,
                DataNascimento = dto.DataNascimento,
                Nacionalidade = dto.Nacionalidade,
                ETitular = dto.ETitular
            };

            var (sucesso, erro) = await _reservaService.AdicionarParticipanteAsync(id, participante);

            if (!sucesso)
                return BadRequest(new { erro });

            return Ok(new { mensagem = "Participante adicionado com sucesso.", participante.Id });
        }

        [HttpGet("{id}/validar-participantes")]
        public async Task<IActionResult> ValidarParticipantes(int id)
        {
            if (!await PodeAcederAsync(id))
                return Forbid();

            var (valido, erro) = await _reservaService.ValidarParticipantesAsync(id);
            return Ok(new { valido, erro });
        }

        [HttpPut("{id}/confirmar")]
        public async Task<IActionResult> Confirmar(int id)
        {
            if (!await PodeAcederAsync(id))
                return Forbid();

            var (sucesso, erro) = await _reservaService.ConfirmarReservaAsync(id);

            if (!sucesso)
                return BadRequest(new { erro });

            return Ok(new { mensagem = "Reserva confirmada com sucesso." });
        }

        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            if (!await PodeAcederAsync(id))
                return Forbid();

            var (sucesso, erro, reembolso) = await _reservaService.CancelarReservaAsync(id);

            if (!sucesso)
                return BadRequest(new { erro });

            return Ok(new
            {
                mensagem = "Reserva cancelada.",
                valorReembolso = reembolso,
                observacao = reembolso > 0
                    ? "Será emitida nota de crédito."
                    : "Cancelamento fora do prazo — sem direito a reembolso."
            });
        }

        /// <summary>Voucher da reserva em PDF.</summary>
        [HttpGet("{id}/voucher")]
        public async Task<IActionResult> DescarregarVoucher(int id)
        {
            if (!await PodeAcederAsync(id))
                return Forbid();

            var reserva = await _reservaService.ObterPorIdAsync(id);
            if (reserva is null)
                return NotFound(new { erro = "Reserva não encontrada." });

            var utilizador = await _userManager.FindByIdAsync(reserva.UtilizadorId.ToString());

            var pdf = _pdfService.GerarVoucher(
                reserva,
                utilizador?.Nome ?? "Cliente",
                utilizador?.Email ?? "");

            return File(pdf, "application/pdf", $"voucher-{reserva.Id:D5}.pdf");
        }

        /// <summary>Exporta todas as reservas em XML. Só staff.</summary>
        [HttpGet("exportar-xml")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Colaborador}")]
        public async Task<IActionResult> ExportarXml()
        {
            var reservas = await _reservaService.ObterTodasComDetalhesAsync();
            var xml = _xmlService.ExportarReservas(reservas);

            return File(xml, "application/xml",
                $"reservas-{DateTime.Now:yyyy-MM-dd}.xml");
        }

        // ── Verificação de propriedade ────────────────────

        private async Task<bool> PodeAcederAsync(int reservaId)
        {
            if (User.EhStaff()) return true;
            return await _reservaService.PertenceAoUtilizadorAsync(reservaId, User.ObterUtilizadorId());
        }
    }
}