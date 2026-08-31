using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItinerarioController : ControllerBase
    {
        private readonly ItinerarioService _itinerarioService;

        public ItinerarioController(ItinerarioService itinerarioService)
        {
            _itinerarioService = itinerarioService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Itinerario>>> ObterTodos()
        {
            var itinerarios = await _itinerarioService.ObterTodosAsync();
            return Ok(itinerarios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Itinerario>> ObterPorId(int id)
        {
            var itinerario = await _itinerarioService.ObterPorIdAsync(id);
            if (itinerario == null) return NotFound("Itinerário não encontrado.");
            return Ok(itinerario);
        }

        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] Itinerario itinerario)
        {
            await _itinerarioService.AdicionarAsync(itinerario);
            return CreatedAtAction(nameof(ObterPorId), new { id = itinerario.Id }, itinerario);
        }
    }
}