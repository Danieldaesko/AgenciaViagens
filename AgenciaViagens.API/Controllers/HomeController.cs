using Microsoft.AspNetCore.Mvc;
using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;


namespace AgenciaViagens.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservaController : ControllerBase
    {
        private readonly ReservaService _reservaService;

        public ReservaController(ReservaService reservaService)
        {
            _reservaService = reservaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reserva>>> ObterTodas()
        {
            var reservas = await _reservaService.ObterTodasAsync();
            return Ok(reservas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> ObterPorId(int id)
        {
            var reserva = await _reservaService.ObterPorIdAsync(id);
            if (reserva == null) return NotFound("Reserva não encontrada.");

            return Ok(reserva);
        }

        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] Reserva reserva)
        {
            await _reservaService.AdicionarAsync(reserva);
            return CreatedAtAction(nameof(ObterPorId), new { id = reserva.Id }, reserva);
        }
    }
}