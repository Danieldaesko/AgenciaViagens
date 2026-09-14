using AgenciaViagens.API.Extensions;
using AgenciaViagens.Application.DTOs;
using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Avaliacao>>> ObterTodas()
        {
            var avaliacoes = await _avaliacaoService.ObterTodasAsync();
            return Ok(avaliacoes);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Avaliacao>> ObterPorId(int id)
        {
            var avaliacao = await _avaliacaoService.ObterPorIdAsync(id);
            if (avaliacao == null) return NotFound("Avaliação não encontrada.");
            return Ok(avaliacao);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Avaliacao>> Criar([FromBody] CriarAvaliacaoDTO dto)
        {
            if (dto.Classificacao < 1 || dto.Classificacao > 5)
                return BadRequest(new { erro = "A classificação deve estar entre 1 e 5." });

            var avaliacao = new Avaliacao
            {
                UtilizadorId = User.ObterUtilizadorId(),
                PacoteId = dto.PacoteId,
                Classificacao = dto.Classificacao,
                Comentario = dto.Comentario,
                Aprovada = false
            };

            await _avaliacaoService.AdicionarAsync(avaliacao);
            return CreatedAtAction(nameof(ObterPorId), new { id = avaliacao.Id }, avaliacao);
        }
    }
}