using AgenciaViagens.Application.Services;
using AgenciaViagens.Domain.Entities;
using AgenciaViagens.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaViagens.Web.Controllers
{
    [Authorize(Roles = $"{Roles.Admin},{Roles.Colaborador}")]
    public class AdminController : Controller
    {
        private readonly EstatisticasService _estatisticas;
        private readonly PacoteService _pacotes;
        private readonly ReservaService _reservas;
        private readonly AvaliacaoService _avaliacoes;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanhoMaximo = 5 * 1024 * 1024; // 5 MB

        public AdminController(
            EstatisticasService estatisticas,
            PacoteService pacotes,
            ReservaService reservas,
            AvaliacaoService avaliacoes,
            IWebHostEnvironment env)
        {
            _estatisticas = estatisticas;
            _pacotes = pacotes;
            _reservas = reservas;
            _avaliacoes = avaliacoes;
            _env = env;
        }

        // ── DASHBOARD ─────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Painel";

            var resumo = await _estatisticas.ObterResumoAsync();

            ViewBag.Mensal = await _estatisticas.ObterFaturacaoMensalAsync(6);
            ViewBag.Categorias = await _estatisticas.ObterReservasPorCategoriaAsync();
            ViewBag.Destinos = await _estatisticas.ObterTopDestinosAsync(5);

            return View(resumo);
        }

        // ── PACOTES ───────────────────────────────────────

        public async Task<IActionResult> Pacotes()
        {
            ViewData["Title"] = "Pacotes";
            var pacotes = await _pacotes.ObterTodosParaGestaoAsync();
            return View(pacotes);
        }

        [HttpGet]
        public IActionResult NovoPacote()
        {
            ViewData["Title"] = "Novo pacote";
            return View(new Pacote
            {
                DataPartida = DateTime.Today.AddMonths(2),
                DataRegresso = DateTime.Today.AddMonths(2).AddDays(7),
                VagasTotal = 10,
                Ativo = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovoPacote(Pacote pacote, IFormFile? imagem)
        {
            ViewData["Title"] = "Novo pacote";

            if (pacote.DataRegresso <= pacote.DataPartida)
                ModelState.AddModelError(nameof(pacote.DataRegresso),
                    "O regresso tem de ser posterior à partida.");

            var erroImagem = ValidarImagem(imagem);
            if (erroImagem is not null)
                ModelState.AddModelError("imagem", erroImagem);

            if (!ModelState.IsValid)
                return View(pacote);

            if (imagem is not null)
                pacote.ImagemUrl = await GuardarImagemAsync(imagem);

            pacote.VagasOcupadas = 0;
            pacote.Ativo = true;
            await _pacotes.AdicionarAsync(pacote);

            TempData["Sucesso"] = "Pacote criado.";
            return RedirectToAction(nameof(Pacotes));
        }

        [HttpGet]
        public async Task<IActionResult> EditarPacote(int id)
        {
            var pacote = await _pacotes.ObterPorIdAsync(id);
            if (pacote is null) return NotFound();

            ViewData["Title"] = "Editar pacote";
            return View(pacote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPacote(Pacote pacote, IFormFile? imagem)
        {
            ViewData["Title"] = "Editar pacote";

            if (pacote.DataRegresso <= pacote.DataPartida)
                ModelState.AddModelError(nameof(pacote.DataRegresso),
                    "O regresso tem de ser posterior à partida.");

            var erroImagem = ValidarImagem(imagem);
            if (erroImagem is not null)
                ModelState.AddModelError("imagem", erroImagem);

            if (!ModelState.IsValid)
                return View(pacote);

            var imagemAntiga = pacote.ImagemUrl;

            if (imagem is not null)
                pacote.ImagemUrl = await GuardarImagemAsync(imagem);

            var (sucesso, erro) = await _pacotes.AtualizarAsync(pacote);

            if (!sucesso)
            {
                ModelState.AddModelError(string.Empty, erro!);
                return View(pacote);
            }

            // Só apaga a antiga depois de a nova estar gravada
            if (imagem is not null)
                ApagarImagem(imagemAntiga);

            TempData["Sucesso"] = "Pacote atualizado.";
            return RedirectToAction(nameof(Pacotes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarPacote(int id)
        {
            var (sucesso, erro) = await _pacotes.AlternarAtivoAsync(id);

            if (!sucesso) TempData["Erro"] = erro;
            else TempData["Sucesso"] = "Estado do pacote alterado.";

            return RedirectToAction(nameof(Pacotes));
        }

        // ── RESERVAS ──────────────────────────────────────

        public async Task<IActionResult> Reservas(string? estado = null)
        {
            ViewData["Title"] = "Reservas";

            var todas = await _reservas.ObterTodasAsync();

            if (!string.IsNullOrEmpty(estado))
                todas = todas.Where(r => r.Estado == estado).ToList();

            ViewBag.EstadoFiltro = estado;
            return View(todas.OrderByDescending(r => r.DataReserva).ToList());
        }

        public async Task<IActionResult> DetalheReserva(int id)
        {
            var reserva = await _reservas.ObterPorIdAsync(id);
            if (reserva is null) return NotFound();

            ViewData["Title"] = $"Reserva #{id}";
            return View(reserva);
        }

        // ── AVALIAÇÕES ────────────────────────────────────

        public async Task<IActionResult> Avaliacoes()
        {
            ViewData["Title"] = "Avaliações";
            var pendentes = await _avaliacoes.ObterPendentesAsync();
            return View(pendentes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarAvaliacao(int id)
        {
            var (sucesso, erro) = await _avaliacoes.AprovarAsync(id);

            if (!sucesso) TempData["Erro"] = erro;
            else TempData["Sucesso"] = "Avaliação publicada.";

            return RedirectToAction(nameof(Avaliacoes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarAvaliacao(int id)
        {
            var (sucesso, erro) = await _avaliacoes.RejeitarAsync(id);

            if (!sucesso) TempData["Erro"] = erro;
            else TempData["Sucesso"] = "Avaliação removida.";

            return RedirectToAction(nameof(Avaliacoes));
        }

        // ── IMAGENS ───────────────────────────────────────

        private static string? ValidarImagem(IFormFile? ficheiro)
        {
            if (ficheiro is null || ficheiro.Length == 0)
                return null;

            var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();

            if (!ExtensoesPermitidas.Contains(extensao))
                return "A imagem tem de ser JPG, PNG ou WEBP.";

            if (ficheiro.Length > TamanhoMaximo)
                return "A imagem não pode ter mais de 5 MB.";

            return null;
        }

        private async Task<string> GuardarImagemAsync(IFormFile ficheiro)
        {
            var pasta = Path.Combine(_env.WebRootPath, "images", "pacotes");
            Directory.CreateDirectory(pasta);

            var extensao = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
            var nome = $"{Guid.NewGuid():N}{extensao}";

            await using var stream = new FileStream(Path.Combine(pasta, nome), FileMode.Create);
            await ficheiro.CopyToAsync(stream);

            return $"/images/pacotes/{nome}";
        }

        private void ApagarImagem(string? url)
        {
            // Só apaga imagens carregadas pelo backoffice, nunca as de /images/ originais
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/images/pacotes/"))
                return;

            var caminho = Path.Combine(_env.WebRootPath,
                url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(caminho))
                System.IO.File.Delete(caminho);
        }
    }
}