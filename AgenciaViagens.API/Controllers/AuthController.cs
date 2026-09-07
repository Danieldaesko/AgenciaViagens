using AgenciaViagens.Application.DTOs;
using AgenciaViagens.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace AgenciaViagens.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    // POST: api/auth/registo
    [HttpPost("registo")]
    public async Task<IActionResult> Registo([FromBody] RegistoDTO dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return BadRequest(new { erro = "Já existe uma conta com este email." });

        var utilizador = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nome = dto.Nome,
            NIF = dto.NIF,
            PhoneNumber = dto.Telefone,
            DataNascimento = dto.DataNascimento,
            EmailConfirmed = true,   // pôr a false quando o EmailService estiver pronto
            Ativo = true
        };

        var resultado = await _userManager.CreateAsync(utilizador, dto.Password);

        if (!resultado.Succeeded)
            return BadRequest(new { erros = resultado.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(utilizador, Roles.Cliente);

        return Ok(new
        {
            mensagem = "Conta criada com sucesso.",
            utilizadorId = utilizador.Id
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginRespostaDTO>> Login([FromBody] LoginDTO dto)
    {
        var utilizador = await _userManager.FindByEmailAsync(dto.Email);

        if (utilizador is null || !await _userManager.CheckPasswordAsync(utilizador, dto.Password))
            return Unauthorized(new LoginRespostaDTO
            {
                Sucesso = false,
                Mensagem = "Email ou password incorretos."
            });

        if (!utilizador.Ativo)
            return Unauthorized(new LoginRespostaDTO
            {
                Sucesso = false,
                Mensagem = "Esta conta está desativada."
            });

        var roles = await _userManager.GetRolesAsync(utilizador);
        var (token, expiracao) = _tokenService.GerarToken(utilizador, roles);

        utilizador.UltimoLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(utilizador);

        return Ok(new LoginRespostaDTO
        {
            Sucesso = true,
            Token = token,
            Expiracao = expiracao,
            UtilizadorId = utilizador.Id,
            Nome = utilizador.Nome,
            Email = utilizador.Email,
            Roles = roles.ToList(),
            Pontos = utilizador.Pontos
        });
    }
}
