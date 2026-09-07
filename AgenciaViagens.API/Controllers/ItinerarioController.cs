using AgenciaViagens.Application.DTOs;
using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AgenciaViagens.Infrastructure.Identity;

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
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Itinerario>>> ObterTodos()
        {
            var itinerarios = await _itinerarioService.ObterTodosAsync();
            return Ok(itinerarios);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Itinerario>> ObterPorId(int id)
        {
            var itinerario = await _itinerarioService.ObterPorIdAsync(id);
            if (itinerario == null) return NotFound("Itinerário não encontrado.");
            return Ok(itinerario);
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Colaborador}")]
        public async Task<ActionResult<Itinerario>> Criar([FromBody] CriarItinerarioDTO dto)
        {
            var itinerario = new Itinerario
            {
                PacoteId = dto.PacoteId,
                Dia = dto.Dia,
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Local = dto.Local
            };

            await _itinerarioService.AdicionarAsync(itinerario);
            return CreatedAtAction(nameof(ObterPorId), new { id = itinerario.Id }, itinerario);
        }
    }
}