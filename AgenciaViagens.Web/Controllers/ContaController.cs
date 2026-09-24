using AgenciaViagens.Infrastructure.Identity;
using AgenciaViagens.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace AgenciaViagens.Web.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ContaController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ══ REGISTO ══════════════════════════════════════

        [HttpGet]
        public IActionResult Registo()
        {
            ViewData["Title"] = "Criar conta";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registo(RegistoVM vm, [FromServices] EmailService email)
        {
            ViewData["Title"] = "Criar conta";

            if (!ModelState.IsValid)
                return View(vm);

            if (await _userManager.FindByEmailAsync(vm.Email) is not null)
            {
                ModelState.AddModelError(string.Empty, "Já existe uma conta com este email.");
                return View(vm);
            }

            var utilizador = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                Nome = vm.Nome,
                NIF = vm.NIF,
                PhoneNumber = vm.Telefone,
                EmailConfirmed = false,   // fica por confirmar até clicar no link
                Ativo = true
            };

            var resultado = await _userManager.CreateAsync(utilizador, vm.Password);

            if (!resultado.Succeeded)
            {
                foreach (var e in resultado.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(utilizador, Roles.Cliente);
            await email.EnviarBoasVindasAsync(utilizador.Email!, utilizador.Nome);
            await EnviarLinkAtivacaoAsync(utilizador, email);

            return RedirectToAction(nameof(ConfirmeEmail), new { email = utilizador.Email });
        }

        // ══ ATIVAÇÃO DE CONTA ════════════════════════════

        [HttpGet]
        public IActionResult ConfirmeEmail(string email)
        {
            ViewData["Title"] = "Confirme o email";
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Ativar(string userId, string token)
        {
            ViewData["Title"] = "Ativação";

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return View("AtivacaoFalhou");

            var utilizador = await _userManager.FindByIdAsync(userId);
            if (utilizador is null)
                return View("AtivacaoFalhou");

            var tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var resultado = await _userManager.ConfirmEmailAsync(utilizador, tokenDecodificado);

            if (!resultado.Succeeded)
                return View("AtivacaoFalhou");

            await _signInManager.SignInAsync(utilizador, isPersistent: false);
            return View("AtivacaoOk");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarAtivacao(string email, [FromServices] EmailService emailService)
        {
            var utilizador = await _userManager.FindByEmailAsync(email);

            // Não revelamos se o email existe ou não
            if (utilizador is not null && !await _userManager.IsEmailConfirmedAsync(utilizador))
                await EnviarLinkAtivacaoAsync(utilizador, emailService);

            TempData["Sucesso"] = "Se a conta existir e ainda não estiver ativa, enviámos um novo link.";
            return RedirectToAction(nameof(ConfirmeEmail), new { email });
        }

        // ══ LOGIN ════════════════════════════════════════

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["Title"] = "Entrar";
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            ViewData["Title"] = "Entrar";
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            var utilizador = await _userManager.FindByEmailAsync(vm.Email);

            if (utilizador is not null && !await _userManager.IsEmailConfirmedAsync(utilizador))
            {
                ModelState.AddModelError(string.Empty, "Ainda não confirmou o seu email. Verifique a sua caixa de entrada.");
                ViewBag.EmailPorConfirmar = vm.Email;
                return View(vm);
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                vm.Email, vm.Password, vm.Lembrar, lockoutOnFailure: false);

            if (!resultado.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Email ou palavra-passe incorretos.");
                return View(vm);
            }

            if (utilizador is not null)
            {
                utilizador.UltimoLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(utilizador);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ══ RECUPERAÇÃO DE PALAVRA-PASSE ═════════════════

        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            ViewData["Title"] = "Recuperar acesso";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(RecuperarVM vm, [FromServices] EmailService email)
        {
            ViewData["Title"] = "Recuperar acesso";

            if (!ModelState.IsValid)
                return View(vm);

            var utilizador = await _userManager.FindByEmailAsync(vm.Email);

            // Só envia se a conta existir e estiver confirmada — mas a resposta é sempre igual
            if (utilizador is not null && await _userManager.IsEmailConfirmedAsync(utilizador))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(utilizador);
                var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var link = Url.Action(nameof(RedefinirPassword), "Conta",
                    new { userId = utilizador.Id, token = tokenCodificado },
                    Request.Scheme)!;

                await email.EnviarRecuperacaoAsync(utilizador.Email!, utilizador.Nome, link);
            }

            return RedirectToAction(nameof(RecuperarEnviado));
        }

        [HttpGet]
        public IActionResult RecuperarEnviado()
        {
            ViewData["Title"] = "Verifique o email";
            return View();
        }

        [HttpGet]
        public IActionResult RedefinirPassword(string userId, string token)
        {
            ViewData["Title"] = "Nova palavra-passe";

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return View("AtivacaoFalhou");

            return View(new RedefinirVM { UserId = userId, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirPassword(RedefinirVM vm)
        {
            ViewData["Title"] = "Nova palavra-passe";

            if (!ModelState.IsValid)
                return View(vm);

            var utilizador = await _userManager.FindByIdAsync(vm.UserId);
            if (utilizador is null)
                return View("AtivacaoFalhou");

            var tokenDecodificado = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(vm.Token));
            var resultado = await _userManager.ResetPasswordAsync(utilizador, tokenDecodificado, vm.Password);

            if (!resultado.Succeeded)
            {
                foreach (var e in resultado.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(vm);
            }

            TempData["Sucesso"] = "Palavra-passe alterada. Já pode entrar.";
            return RedirectToAction(nameof(Login));
        }

        // ══ PERFIL / ACESSO ══════════════════════════════

        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            ViewData["Title"] = "A minha conta";
            var utilizador = await _userManager.GetUserAsync(User);
            if (utilizador is null) return NotFound();
            return View(utilizador);
        }

        public IActionResult AcessoNegado()
        {
            ViewData["Title"] = "Sem acesso";
            return View();
        }

        // ── Auxiliar ──────────────────────────────────────

        private async Task EnviarLinkAtivacaoAsync(ApplicationUser utilizador, EmailService email)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(utilizador);
            var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var link = Url.Action(nameof(Ativar), "Conta",
                new { userId = utilizador.Id, token = tokenCodificado },
                Request.Scheme)!;

            await email.EnviarAtivacaoAsync(utilizador.Email!, utilizador.Nome, link);
        }
    }

    // ══ VIEW MODELS ══════════════════════════════════════

    public class RegistoVM
    {
        [Required(ErrorMessage = "Indique o seu nome.")]
        [Display(Name = "Nome completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indique o seu email.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escolha uma palavra-passe.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "A palavra-passe precisa de pelo menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "As palavras-passe não coincidem.")]
        [Display(Name = "Repetir palavra-passe")]
        public string ConfirmarPassword { get; set; } = string.Empty;

        public string? NIF { get; set; }
        public string? Telefone { get; set; }
    }

    public class LoginVM
    {
        [Required(ErrorMessage = "Indique o seu email.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indique a palavra-passe.")]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Manter sessão iniciada")]
        public bool Lembrar { get; set; }
    }

    public class RecuperarVM
    {
        [Required(ErrorMessage = "Indique o seu email.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;
    }

    public class RedefinirVM
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escolha uma palavra-passe.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Pelo menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova palavra-passe")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "As palavras-passe não coincidem.")]
        [Display(Name = "Repetir palavra-passe")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
} 