using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColetas.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _service;

    public VeiculosController(IVeiculoService service) => _service = service;

    /// <summary>Lista os veículos cadastrados.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VeiculoResponse>>> Listar() =>
        Ok(await _service.ListarAsync());

    /// <summary>Cadastra um novo veículo.</summary>
    [HttpPost]
    public async Task<ActionResult<VeiculoResponse>> Criar([FromBody] CriarVeiculoRequest request)
    {
        var veiculo = await _service.CriarAsync(request);
        return Created($"/api/veiculos/{veiculo.Id}", veiculo);
    }
}
