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
    public class FaturaController : ControllerBase
    {
        private readonly FaturaService _faturaService;

        public FaturaController(FaturaService faturaService)
        {
            _faturaService = faturaService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Fatura>> ObterPorId(int id)
        {
            var fatura = await _faturaService.ObterPorIdAsync(id);
            if (fatura is null)
                return NotFound(new { erro = "Fatura não encontrada." });
            return Ok(fatura);
        }

        [HttpGet("reserva/{reservaId}")]
        public async Task<ActionResult<List<Fatura>>> ObterPorReserva(int reservaId)
        {
            var faturas = await _faturaService.ObterPorReservaAsync(reservaId);
            return Ok(faturas);
        }

        [HttpPost("reserva/{reservaId}/emitir")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Colaborador}")]
        public async Task<IActionResult> Emitir(int reservaId, [FromQuery] string tipo = "Fatura")
        {
            var (sucesso, erro, fatura) = await _faturaService.EmitirFaturaAsync(reservaId, tipo);

            if (!sucesso)
                return BadRequest(new { erro });

            return Ok(new
            {
                mensagem = "Fatura emitida com sucesso.",
                faturaId = fatura!.Id,
                numero = fatura.Numero,
                valorTotal = fatura.ValorTotal
            });
        }

        [HttpPut("{id}/anular")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Anular(int id)
        {
            var (sucesso, erro) = await _faturaService.AnularFaturaAsync(id);
            if (!sucesso)
                return BadRequest(new { erro });
            return Ok(new { mensagem = "Fatura anulada." });
        }
    }
}