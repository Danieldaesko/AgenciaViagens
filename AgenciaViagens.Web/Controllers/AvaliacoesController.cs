using AgenciaViagens.Application.Services;
using AgenciaViagens.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AgenciaViagens.Web.Controllers
{
    [Authorize]
    public class AvaliacoesController : Controller
    {
        private readonly AvaliacaoService _avaliacaoService;
        private readonly ReservaService _reservaService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AvaliacoesController(
            AvaliacaoService avaliacaoService,
            ReservaService reservaService,
            UserManager<ApplicationUser> userManager)
        {
            _avaliacaoService = avaliacaoService;
            _reservaService = reservaService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Nova(int reservaId)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var (pode, motivo) = await _avaliacaoService.PodeAvaliarAsync(reservaId, utilizador.Id);
            if (!pode)
            {
                TempData["Erro"] = motivo;
                return RedirectToAction("Detalhe", "Reservas", new { id = reservaId });
            }

            var reserva = await _reservaService.ObterDoUtilizadorAsync(reservaId, utilizador.Id);

            ViewData["Title"] = "Avaliar viagem";
            ViewBag.Reserva = reserva;
            return View(new AvaliacaoVM { ReservaId = reservaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nova(AvaliacaoVM vm)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(vm.ReservaId, utilizador.Id);
            if (reserva is null) return NotFound();

            ViewData["Title"] = "Avaliar viagem";
            ViewBag.Reserva = reserva;

            if (!ModelState.IsValid)
                return View(vm);

            var (sucesso, erro) = await _avaliacaoService.AvaliarAsync(
                vm.ReservaId, utilizador.Id, vm.Classificacao, vm.Comentario);

            if (!sucesso)
            {
                ModelState.AddModelError(string.Empty, erro!);
                return View(vm);
            }

            TempData["Sucesso"] = "Obrigado. A sua avaliação fica visível depois de revista pela equipa.";
            return RedirectToAction("Detalhe", "Reservas", new { id = vm.ReservaId });
        }

        public async Task<IActionResult> Minhas()
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            ViewData["Title"] = "As minhas avaliações";
            var avaliacoes = await _avaliacaoService.ObterDoUtilizadorAsync(utilizador.Id);
            return View(avaliacoes);
        }
    }

    public class AvaliacaoVM
    {
        public int ReservaId { get; set; }

        [Range(1, 5, ErrorMessage = "Escolha entre 1 e 5 estrelas.")]
        public int Classificacao { get; set; }

        [Required(ErrorMessage = "Conte-nos como correu.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Escreva entre 10 e 1000 caracteres.")]
        public string Comentario { get; set; } = string.Empty;
    }
}