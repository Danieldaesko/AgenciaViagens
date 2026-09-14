using AgenciaViagens.Application.DTOs;
using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Identity;
using AgenciaViagens.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.API.Controllers
{
    [Tags("Pacote")]
    [Route("api/[controller]")]
    [ApiController]
    public class PacoteController : ControllerBase
    {
        private readonly PacoteService _pacoteService;
        private readonly PdfService _pdfService;

        private readonly XmlExportService _xmlService;
        public PacoteController(
           PacoteService pacoteService,
           PdfService pdfService,
           XmlExportService xmlService)
        {
            _pacoteService = pacoteService;
            _pdfService = pdfService;
            _xmlService = xmlService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Pacote>>> ObterTodos()
        {
            var pacotes = await _pacoteService.ObterTodosAsync();
            return Ok(pacotes);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Pacote>> ObterPorId(int id)
        {
            var pacote = await _pacoteService.ObterPorIdAsync(id);
            if (pacote == null) return NotFound("Pacote não encontrado.");
            return Ok(pacote);
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Colaborador}")]
        public async Task<ActionResult<Pacote>> Criar([FromBody] CriarPacoteDTO dto)
        {
            if (dto.DataRegresso <= dto.DataPartida)
                return BadRequest(new { erro = "A data de regresso deve ser posterior à data de partida." });

            var pacote = new Pacote
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Origem = dto.Origem,
                Destino = dto.Destino,
                Pais = dto.Pais,
                Categoria = dto.Categoria,
                PrecoBase = dto.PrecoBase,
                PrecoPromocao = dto.PrecoPromocao,
                DataPartida = dto.DataPartida,
                DataRegresso = dto.DataRegresso,
                VagasTotal = dto.VagasTotal,
                VagasOcupadas = 0,
                ImagemUrl = dto.ImagemUrl,
                EmDestaque = dto.EmDestaque,
                EmPromocao = dto.EmPromocao,
                Ativo = true
            };

            await _pacoteService.AdicionarAsync(pacote);
            return CreatedAtAction(nameof(ObterPorId), new { id = pacote.Id }, pacote);
        }

        [HttpGet("pesquisar")]
        [AllowAnonymous]
        public async Task<ActionResult<ResultadoPaginadoDTO<Pacote>>> Pesquisar([FromQuery] FiltroPacotesDTO filtro)
        {
            if (filtro.Pagina < 1) filtro.Pagina = 1;
            if (filtro.TamanhoPagina is < 1 or > 50) filtro.TamanhoPagina = 9;

            var resultado = await _pacoteService.PesquisarAsync(filtro);
            return Ok(resultado);
        }

        [HttpGet("destaques")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Pacote>>> ObterDestaques([FromQuery] int limite = 6)
        {
            var pacotes = await _pacoteService.ObterDestaquesAsync(limite);
            return Ok(pacotes);
        }

        [HttpGet("promocoes")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Pacote>>> ObterPromocoes([FromQuery] int limite = 6)
        {
            var pacotes = await _pacoteService.ObterPromocoesAsync(limite);
            return Ok(pacotes);
        }

        [HttpGet("categorias")]
        [AllowAnonymous]
        public async Task<ActionResult<List<string>>> ObterCategorias()
        {
            var categorias = await _pacoteService.ObterCategoriasAsync();
            return Ok(categorias);
        }

        [HttpGet("destinos")]
        [AllowAnonymous]
        public async Task<ActionResult<List<string>>> ObterDestinos()
        {
            var destinos = await _pacoteService.ObterDestinosAsync();
            return Ok(destinos);
        }

        /// <summary>Itinerário do pacote em PDF.</summary>
        [HttpGet("{id}/itinerario-pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> DescarregarItinerario(int id)
        {
            var pacote = await _pacoteService.ObterPorIdAsync(id);
            if (pacote is null)
                return NotFound(new { erro = "Pacote não encontrado." });

            var pdf = _pdfService.GerarItinerario(pacote);
            var nome = pacote.Destino.ToLower().Replace(" ", "-");

            return File(pdf, "application/pdf", $"itinerario-{nome}.pdf");
        }


        /// <summary>Exporta os pacotes em XML. Só staff.</summary>
        [HttpGet("exportar-xml")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Colaborador}")]
        public async Task<IActionResult> ExportarXml()
        {
            var pacotes = await _pacoteService.ObterTodosParaGestaoAsync();
            var xml = _xmlService.ExportarPacotes(pacotes);

            return File(xml, "application/xml",
                $"pacotes-{DateTime.Now:yyyy-MM-dd}.xml");
        }
    }
}