using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColetas.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service) => _service = service;

    /// <summary>Lista os clientes cadastrados.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteResponse>>> Listar() =>
        Ok(await _service.ListarAsync());

    /// <summary>Cadastra um novo cliente.</summary>
    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar([FromBody] CriarClienteRequest request)
    {
        var cliente = await _service.CriarAsync(request);
        return Created($"/api/clientes/{cliente.Id}", cliente);
    }
}
