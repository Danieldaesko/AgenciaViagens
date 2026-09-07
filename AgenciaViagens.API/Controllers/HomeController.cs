using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly PacoteService _pacoteService;

        public HomeController(PacoteService pacoteService)
        {
            _pacoteService = pacoteService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Viagens organizadas";

            var destaques = await _pacoteService.ObterDestaquesAsync(6);
            var destinos = await _pacoteService.ObterDestinosAsync();

            ViewBag.Destinos = destinos;

            return View(destaques);
        }

        public IActionResult Sobre()
        {
            ViewData["Title"] = "Sobre nós";
            return View();
        }

        public IActionResult Contacto()
        {
            ViewData["Title"] = "Contacto";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}