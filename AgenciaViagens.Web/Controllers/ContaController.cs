using AgenciaViagens.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

        [HttpGet]
        public IActionResult Registo()
        {
            ViewData["Title"] = "Criar conta";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registo(RegistoVM vm)
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
                EmailConfirmed = true,
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
            await _signInManager.SignInAsync(utilizador, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

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

            var resultado = await _signInManager.PasswordSignInAsync(
                vm.Email, vm.Password, vm.Lembrar, lockoutOnFailure: false);

            if (!resultado.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Email ou palavra-passe incorretos.");
                return View(vm);
            }

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user is not null)
            {
                user.UltimoLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
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
    }

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
}