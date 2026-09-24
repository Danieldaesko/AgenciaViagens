using System.Text.RegularExpressions;
using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.Web.Controllers
{
    [Authorize]
    public class PagamentosController : Controller
    {
        private readonly ReservaService _reservaService;
        private readonly PagamentoService _pagamentoService;
        private readonly FaturaService _faturaService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PagamentosController(
            ReservaService reservaService,
            PagamentoService pagamentoService,
            FaturaService faturaService,
            UserManager<ApplicationUser> userManager)
        {
            _reservaService = reservaService;
            _pagamentoService = pagamentoService;
            _faturaService = faturaService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Nova(int reservaId)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(reservaId, utilizador.Id);
            if (reserva is null) return NotFound();

            var bloqueio = await VerificarSePodePagarAsync(reserva);
            if (bloqueio is not null)
            {
                TempData["Erro"] = bloqueio;
                return RedirectToAction("Detalhe", "Reservas", new { id = reservaId });
            }

            await PrepararViewAsync(reserva);
            return View(new PagamentoVM { ReservaId = reservaId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nova(PagamentoVM vm)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var reserva = await _reservaService.ObterDoUtilizadorAsync(vm.ReservaId, utilizador.Id);
            if (reserva is null) return NotFound();

            var bloqueio = await VerificarSePodePagarAsync(reserva);
            if (bloqueio is not null)
            {
                TempData["Erro"] = bloqueio;
                return RedirectToAction("Detalhe", "Reservas", new { id = vm.ReservaId });
            }

            var disponivel = await _pagamentoService.ObterValorDisponivelAsync(vm.ReservaId);
            var valor = vm.Modalidade == "sinal" && PodePagarSinal(reserva)
                ? Math.Min(Math.Round(reserva.PrecoTotal * 0.30m, 2), disponivel)
                : disponivel;

            string metodo;
            string? referencia;
            string estado;

            switch (vm.Metodo)
            {
                case "cartao":
                    ValidarCartao(vm);
                    metodo = $"Cartão {ValidadorCartao.Bandeira(vm.NumeroCartao)}";
                    referencia = ValidadorCartao.Mascarar(vm.NumeroCartao);
                    estado = "Pago";
                    break;

                case "mbway":
                    var telemovel = ValidadorCartao.Limpar(vm.Telemovel);
                    if (!Regex.IsMatch(telemovel, @"^9[1236]\d{7}$"))
                        ModelState.AddModelError(nameof(vm.Telemovel), "Indique um telemóvel português válido (9 dígitos).");
                    metodo = "MB Way";
                    referencia = telemovel.Length == 9 ? $"{telemovel[..2]}• ••• {telemovel[^3..]}" : null;
                    estado = "Pago";
                    break;

                case "multibanco":
                    metodo = "Multibanco";
                    referencia = $"Ent. {PagamentoService.EntidadeMultibanco} · Ref. {PagamentoService.GerarReferenciaMultibanco(vm.ReservaId, valor)}";
                    estado = "Pendente";
                    break;

                case "transferencia":
                    metodo = "Transferência bancária";
                    referencia = $"TRF-{vm.ReservaId:D5}";
                    estado = "Pendente";
                    break;

                default:
                    ModelState.AddModelError(nameof(vm.Metodo), "Escolha um método de pagamento.");
                    metodo = "";
                    referencia = null;
                    estado = "";
                    break;
            }

            if (!ModelState.IsValid)
            {
                await PrepararViewAsync(reserva);
                return View(vm);
            }

            var (sucesso, erro, pagamento) = await _pagamentoService.RegistarPagamentoAsync(
                vm.ReservaId, valor, metodo, referencia, estado);

            if (!sucesso)
            {
                ModelState.AddModelError(string.Empty, erro!);
                await PrepararViewAsync(reserva);
                return View(vm);
            }

            if (estado == "Pago")
                await EmitirFaturaSeLiquidadaAsync(vm.ReservaId);

            return RedirectToAction(nameof(Resultado), new { id = pagamento!.Id });
        }

        public async Task<IActionResult> Resultado(int id)
        {
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return Challenge();

            var pagamento = await _pagamentoService.ObterPorIdAsync(id);
            if (pagamento is null || pagamento.Reserva.UtilizadorId != utilizador.Id)
                return NotFound();

            ViewData["Title"] = "Pagamento";
            ViewBag.ReferenciaMb = PagamentoService.GerarReferenciaMultibanco(pagamento.ReservaId, pagamento.Valor);
            return View(pagamento);
        }

        // ── Backoffice: confirmar pagamento pendente ─────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Colaborador}")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var (sucesso, erro, reservaId) = await _pagamentoService.ConfirmarPagamentoAsync(id);

            if (!sucesso)
            {
                TempData["Erro"] = erro;
                return reservaId > 0
                    ? RedirectToAction("DetalheReserva", "Admin", new { id = reservaId })
                    : RedirectToAction("Reservas", "Admin");
            }

            await EmitirFaturaSeLiquidadaAsync(reservaId);

            TempData["Sucesso"] = "Pagamento confirmado.";
            return RedirectToAction("DetalheReserva", "Admin", new { id = reservaId });
        }

        // ── Auxiliares ────────────────────────────────────

        private async Task<string?> VerificarSePodePagarAsync(Reserva reserva)
        {
            if (reserva.Estado is "Cancelada" or "Concluida")
                return $"Esta reserva está {reserva.Estado.ToLower()}.";

            if (reserva.Estado == "Pendente")
                return "Confirme a reserva antes de pagar.";

            var disponivel = await _pagamentoService.ObterValorDisponivelAsync(reserva.Id);
            if (disponivel <= 0)
                return "Esta reserva já está paga ou tem um pagamento a aguardar confirmação.";

            return null;
        }

        private static bool PodePagarSinal(Reserva reserva)
            => !reserva.Pagamentos.Any(p => p.Estado is "Pago" or "Pendente");

        private async Task PrepararViewAsync(Reserva reserva)
        {
            ViewData["Title"] = "Pagar reserva";
            ViewBag.Reserva = reserva;
            ViewBag.Disponivel = await _pagamentoService.ObterValorDisponivelAsync(reserva.Id);
            ViewBag.PodeSinal = PodePagarSinal(reserva);
            ViewBag.ValorSinal = Math.Round(reserva.PrecoTotal * 0.30m, 2);
        }

        private void ValidarCartao(PagamentoVM vm)
        {
            if (!ValidadorCartao.NumeroValido(vm.NumeroCartao))
                ModelState.AddModelError(nameof(vm.NumeroCartao), "Número de cartão inválido.");

            if (!ValidadorCartao.ValidadeValida(vm.Validade))
                ModelState.AddModelError(nameof(vm.Validade), "Validade inválida ou cartão expirado (MM/AA).");

            if (!ValidadorCartao.CvvValido(vm.Cvv, vm.NumeroCartao ?? ""))
                ModelState.AddModelError(nameof(vm.Cvv), "CVV inválido.");

            if (string.IsNullOrWhiteSpace(vm.TitularCartao) || vm.TitularCartao.Trim().Length < 3)
                ModelState.AddModelError(nameof(vm.TitularCartao), "Indique o nome como está no cartão.");
        }

        private async Task EmitirFaturaSeLiquidadaAsync(int reservaId)
        {
            var (_, _, _, liquidado) = await _pagamentoService.ObterEstadoPagamentoAsync(reservaId);
            if (!liquidado) return;

            var faturas = await _faturaService.ObterPorReservaAsync(reservaId);
            if (faturas.Any(f => !f.Anulada)) return;

            await _faturaService.EmitirFaturaAsync(reservaId);
        }
    }

    public class PagamentoVM
    {
        public int ReservaId { get; set; }
        public string Metodo { get; set; } = "cartao";
        public string Modalidade { get; set; } = "total";

        public string? NumeroCartao { get; set; }
        public string? Validade { get; set; }
        public string? Cvv { get; set; }
        public string? TitularCartao { get; set; }

        public string? Telemovel { get; set; }
    }
}