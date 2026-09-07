using AgenciaViagens.Application.DTOs;
using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PagamentoController : ControllerBase
    {
        private readonly PagamentoService _pagamentoService;

        public PagamentoController(PagamentoService pagamentoService)
        {
            _pagamentoService = pagamentoService;
        }

        [HttpGet("reserva/{reservaId}")]
        public async Task<ActionResult<List<Pagamento>>> ObterPorReserva(int reservaId)
        {
            var pagamentos = await _pagamentoService.ObterPorReservaAsync(reservaId);
            return Ok(pagamentos);
        }

        [HttpGet("reserva/{reservaId}/estado")]
        public async Task<IActionResult> ObterEstado(int reservaId)
        {
            var (total, pago, emFalta, liquidado) = await _pagamentoService.ObterEstadoPagamentoAsync(reservaId);
            return Ok(new { total, pago, emFalta, liquidado });
        }

        [HttpPost("reserva/{reservaId}")]
        public async Task<IActionResult> Registar(int reservaId, [FromBody] RegistarPagamentoDTO dto)
        {
            var (sucesso, erro, pagamento) = await _pagamentoService.RegistarPagamentoAsync(
                reservaId, dto.Valor, dto.Metodo, dto.Referencia);

            if (!sucesso)
                return BadRequest(new { erro });

            return Ok(new { mensagem = "Pagamento registado.", pagamentoId = pagamento!.Id });
        }
    }
}