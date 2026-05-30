using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColetas.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MotoristasController : ControllerBase
{
    private readonly IMotoristaService _service;

    public MotoristasController(IMotoristaService service) => _service = service;

    /// <summary>Lista todos os motoristas cadastrados.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MotoristaResponse>>> Listar()
    {
        var motoristas = await _service.ListarAsync();
        return Ok(motoristas);
    }

    /// <summary>Busca um motorista pelo id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MotoristaResponse>> ObterPorId(int id)
    {
        var motorista = await _service.ObterPorIdAsync(id);
        return motorista is null ? NotFound() : Ok(motorista);
    }

    /// <summary>Cadastra um novo motorista.</summary>
    [HttpPost]
    public async Task<ActionResult<MotoristaResponse>> Criar([FromBody] CriarMotoristaRequest request)
    {
        var motorista = await _service.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = motorista.Id }, motorista);
    }
}