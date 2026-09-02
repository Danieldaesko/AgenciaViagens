using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservaController : ControllerBase
    {
        private readonly ReservaService _reservaService;

        public ReservaController(ReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Reserva>>> ObterTodas()
        {
            var reservas = await _reservaService.ObterTodasAsync();
            return Ok(reservas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> ObterPorId(int id)
        {
            var reserva = await _reservaService.ObterPorIdAsync(id);
            if (reserva is null)
                return NotFound(new { erro = "Reserva não encontrada." });
            return Ok(reserva);
        }

        [HttpPost]
        public async Task<ActionResult<Reserva>> Criar([FromBody] Reserva reserva)
        {
            await _reservaService.AdicionarAsync(reserva);
            return CreatedAtAction(nameof(ObterPorId), new { id = reserva.Id }, reserva);
        }

        [HttpPost("{id}/participantes")]
        public async Task<IActionResult> AdicionarParticipante(int id, [FromBody] Participante participante)
        {
            var (sucesso, erro) = await _reservaService.AdicionarParticipanteAsync(id, participante);
            if (!sucesso)
                return BadRequest(new { erro });
            return Ok(new { mensagem = "Participante adicionado com sucesso.", participante.Id });
        }

        [HttpGet("{id}/validar-participantes")]
        public async Task<IActionResult> ValidarParticipantes(int id)
        {
            var (valido, erro) = await _reservaService.ValidarParticipantesAsync(id);
            return Ok(new { valido, erro });
        }

        [HttpPut("{id}/confirmar")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var (sucesso, erro) = await _reservaService.ConfirmarReservaAsync(id);
            if (!sucesso)
                return BadRequest(new { erro });
            return Ok(new { mensagem = "Reserva confirmada com sucesso." });
        }

        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
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
    }
}