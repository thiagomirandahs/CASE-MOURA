using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestaoColetas.WebAPI.Controllers;

public record LoginRequest(string Usuario, string Senha);
public record LoginResponse(string Token, DateTime ExpiraEm);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config) => _config = config;

    /// <summary>Autentica e devolve um token JWT. (Demo: usuário "admin", senha "admin123".)</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        // Validação simples para o case. Em produção viria de uma tabela de usuários com senha em hash.
        if (request.Usuario != "admin" || request.Senha != "admin123")
            return Unauthorized(new { erro = "Usuário ou senha inválidos." });

        var jwt = _config.GetSection("Jwt");
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var expiraEm = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiraEmMinutos"] ?? "480"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Usuario),
            new Claim(ClaimTypes.Role, "Operador"),
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiraEm));
    }
}
