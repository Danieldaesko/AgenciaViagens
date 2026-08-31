using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvaliacaoController : ControllerBase
    {
        private readonly AvaliacaoService _avaliacaoService;

        public AvaliacaoController(AvaliacaoService avaliacaoService)
        {
            _avaliacaoService = avaliacaoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Avaliacao>>> ObterTodas()
        {
            var avaliacoes = await _avaliacaoService.ObterTodasAsync();
            return Ok(avaliacoes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Avaliacao>> ObterPorId(int id)
        {
            var avaliacao = await _avaliacaoService.ObterPorIdAsync(id);
            if (avaliacao == null) return NotFound("Avaliação não encontrada.");
            return Ok(avaliacao);
        }

        [HttpPost]
        public async Task<ActionResult> Criar([FromBody] Avaliacao avaliacao)
        {
            await _avaliacaoService.AdicionarAsync(avaliacao);
            return CreatedAtAction(nameof(ObterPorId), new { id = avaliacao.Id }, avaliacao);
        }
    }
}