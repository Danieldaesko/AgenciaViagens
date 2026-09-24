using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Identity;
using AgenciaViagens.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AgenciaViagens.Web.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ReservaService _reservaService;
        private readonly PacoteService _pacoteService;
        private readonly PdfService _pdfService;
        private readonly AvaliacaoService _avaliacaoService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservasController(
            ReservaService reservaService,
            PacoteService pacoteService,
            PdfService pdfService,
            AvaliacaoService avaliacaoService,
            UserManager<ApplicationUser> userManager)
        {
            _reservaService = reservaService;
            _pacoteService = pacoteService;
            _pdfService = pdfService;
            _avaliacaoService = avaliacaoService;
            _userManager = userManager;
        }

        // ── 1. Escolher pessoas e alojamento ──────────────

        [HttpGet]
        public async Task<IActionResult> Nova(int pacoteId)
        {
            var pacote = await _pacoteService.ObterPorIdAsync(pacoteId);
            if (pacote is null) return NotFound();

            var vagas = pacote.VagasTotal - pacote.VagasOcupadas;
            if (vagas <= 0)
            {
                TempData["Erro"] = "Esta partida está esgotada.";
                return RedirectToAction("Detalhe", "Pacotes", new { id = pacoteId });
            }

            ViewData["Title"] = "Reservar " + pacote.Nome;
            ViewBag.Pacote = pacote;

            return View(new NovaReservaVM
            {
                PacoteId = pacoteId,
                NumParticipantes = 1,
                CodigoAlojamento = "standard"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nova(NovaReservaVM vm)
        {
            var pacote = await _pacoteService.ObterPorIdAsync(vm.PacoteId);
            if (pacote is null) return NotFound();

            ViewData["Title"] = "Reservar " + pacote.Nome;
            ViewBag.Pacote = pacote;

            if (!ModelState.IsValid)
                return View(vm);

            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var (sucesso, erro, reservaId) = await _reservaService.CriarReservaAsync(
                utilizador.Id, vm.PacoteId, vm.NumParticipantes, vm.CodigoAlojamento, vm.Observacoes);

            if (!sucesso)
            {
                ModelState.AddModelError(string.Empty, erro!);
                return View(vm);
            }

            return RedirectToAction(nameof(Participantes), new { id = reservaId });
        }

        // ── 2. Preencher os viajantes ─────────────────────

        [HttpGet]
        public async Task<IActionResult> Participantes(int id)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(id, utilizador.Id);
            if (reserva is null) return NotFound();

            ViewData["Title"] = "Quem viaja";
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarParticipante(int id, ParticipanteVM vm)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(id, utilizador.Id);
            if (reserva is null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Quem viaja";
                return View(nameof(Participantes), reserva);
            }

            var participante = new Participante
            {
                ReservaId = id,
                Nome = vm.Nome,
                Documento = vm.Documento,
                TipoDocumento = vm.TipoDocumento,
                DataNascimento = vm.DataNascimento,
                Nacionalidade = vm.Nacionalidade,
                ETitular = vm.ETitular
            };

            var (sucesso, erro) = await _reservaService.AdicionarParticipanteAsync(id, participante);

            if (!sucesso)
                TempData["Erro"] = erro;

            return RedirectToAction(nameof(Participantes), new { id });
        }

        // ── 3. Confirmar ──────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Confirmar(int id)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(id, utilizador.Id);
            if (reserva is null) return NotFound();

            var (valido, erro) = await _reservaService.ValidarParticipantesAsync(id);
            ViewBag.Valido = valido;
            ViewBag.ErroValidacao = erro;

            ViewData["Title"] = "Confirmar reserva";
            return View(reserva);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Confirmar")]
        public async Task<IActionResult> ConfirmarPost(int id, [FromServices] EmailService email)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(id, utilizador.Id);
            if (reserva is null) return NotFound();

            var (sucesso, erro) = await _reservaService.ConfirmarReservaAsync(id);

            if (!sucesso)
            {
                TempData["Erro"] = erro;
                return RedirectToAction(nameof(Confirmar), new { id });
            }

            // Volta a carregar para o voucher já sair com o estado "Confirmada"
            var confirmada = await _reservaService.ObterDoUtilizadorAsync(id, utilizador.Id);
            var voucher = _pdfService.GerarVoucher(confirmada!, utilizador.Nome, utilizador.Email ?? "");
            await email.EnviarReservaConfirmadaAsync(utilizador.Email!, utilizador.Nome, confirmada!, voucher);

            TempData["Sucesso"] = "Reserva confirmada. Enviámos o voucher para o seu email.";
            return RedirectToAction("Nova", "Pagamentos", new { reservaId = id });
        }

        // ── Histórico ─────────────────────────────────────

        public async Task<IActionResult> Minhas()
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            ViewData["Title"] = "As minhas reservas";
            var reservas = await _reservaService.ObterPorUtilizadorAsync(utilizador.Id);
            return View(reservas);
        }

        public async Task<IActionResult> Detalhe(int id)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(id, utilizador.Id);
            if (reserva is null) return NotFound();

            var (podeAvaliar, _) = await _avaliacaoService.PodeAvaliarAsync(id, utilizador.Id);
            ViewBag.PodeAvaliar = podeAvaliar;

            ViewData["Title"] = $"Reserva {reserva.Id}";
            return View(reserva);
        }

        // ── Voucher em PDF ────────────────────────────────

        public async Task<IActionResult> Voucher(int id)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(id, utilizador.Id);
            if (reserva is null) return NotFound();

            var pdf = _pdfService.GerarVoucher(reserva, utilizador.Nome, utilizador.Email ?? "");
            return File(pdf, "application/pdf", $"voucher-{reserva.Id:D5}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(id, utilizador.Id);
            if (reserva is null) return NotFound();

            var (sucesso, erro, reembolso) = await _reservaService.CancelarReservaAsync(id);

            if (!sucesso)
                TempData["Erro"] = erro;
            else
                TempData["Sucesso"] = reembolso > 0
                    ? $"Reserva cancelada. Vai receber {reembolso:C} de volta."
                    : "Reserva cancelada. Sem direito a reembolso por estar fora do prazo.";

            return RedirectToAction(nameof(Detalhe), new { id });
        }
    }

    // ── View Models ───────────────────────────────────────

    public class NovaReservaVM
    {
        public int PacoteId { get; set; }

        [Range(1, 10, ErrorMessage = "Indique entre 1 e 10 viajantes.")]
        [Display(Name = "Quantas pessoas")]
        public int NumParticipantes { get; set; } = 1;

        [Required(ErrorMessage = "Escolha um tipo de alojamento.")]
        public string CodigoAlojamento { get; set; } = "standard";

        [Display(Name = "Alguma nota para nós?")]
        public string? Observacoes { get; set; }
    }

    public class ParticipanteVM
    {
        [Required(ErrorMessage = "Indique o nome.")]
        [Display(Name = "Nome completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indique o documento.")]
        [Display(Name = "Nº do documento")]
        public string Documento { get; set; } = string.Empty;

        [Display(Name = "Tipo")]
        public string TipoDocumento { get; set; } = "CC";

        [Display(Name = "Data de nascimento")]
        public DateTime? DataNascimento { get; set; }

        [Display(Name = "Nacionalidade")]
        public string? Nacionalidade { get; set; }

        [Display(Name = "É o titular da reserva")]
        public bool ETitular { get; set; }
    }
}