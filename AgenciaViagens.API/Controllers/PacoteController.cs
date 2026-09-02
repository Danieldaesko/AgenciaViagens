using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;

namespace AgenciaViagens.API.Controllers
{
    [Tags("Pacote")]
    [Route("api/[controller]")]
    [ApiController]
    public class PacoteController : ControllerBase
    {
        private readonly PacoteService _pacoteService;

        public PacoteController(PacoteService pacoteService)
        {
            _pacoteService = pacoteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pacote>>> ObterTodos()
        {
            var pacotes = await _pacoteService.ObterTodosAsync();
            return Ok(pacotes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pacote>> ObterPorId(int id)
        {
            var pacote = await _pacoteService.ObterPorIdAsync(id);
            if (pacote == null) return NotFound("Pacote não encontrado.");
            return Ok(pacote);
        }

        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] Pacote pacote)
        {
            await _pacoteService.AdicionarAsync(pacote);
            return CreatedAtAction(nameof(ObterPorId), new { id = pacote.Id }, pacote);
        }
    }
}