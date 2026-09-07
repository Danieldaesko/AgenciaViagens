using AgenciaViagens.Application.Services;
using AgenciaViagens.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AgenciaViagens.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly PacoteService _pacoteService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(PacoteService pacoteService, ILogger<HomeController> logger)
        {
            _pacoteService = pacoteService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Viagens organizadas";

            var destaques = await _pacoteService.ObterDestaquesAsync(6);
            ViewBag.Destinos = await _pacoteService.ObterDestinosAsync();
            ViewBag.Origens = await _pacoteService.ObterOrigensAsync();
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}