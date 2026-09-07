using AgenciaViagens.Application.DTOs;
using AgenciaViagens.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.Web.Controllers
{
    public class PacotesController : Controller
    {
        private readonly PacoteService _pacoteService;

        public PacotesController(PacoteService pacoteService)
        {
            _pacoteService = pacoteService;
        }

        public async Task<IActionResult> Index([FromQuery] FiltroPacotesDTO filtro)
        {
            ViewData["Title"] = "Pacotes de viagem";

            if (filtro.Pagina < 1) filtro.Pagina = 1;
            filtro.TamanhoPagina = 9;

            var resultado = await _pacoteService.PesquisarAsync(filtro);

            ViewBag.Filtro = filtro;
            ViewBag.Destinos = await _pacoteService.ObterDestinosAsync();
            ViewBag.Categorias = await _pacoteService.ObterCategoriasAsync();

            return View(resultado);
        }

        public async Task<IActionResult> Detalhe(int id)
        {
            var pacote = await _pacoteService.ObterPorIdAsync(id);

            if (pacote is null)
                return NotFound();

            ViewData["Title"] = pacote.Nome;
            return View(pacote);
        }
    }
}
