using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Services;
using GestaoColetas.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColetas.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ColetasController : ControllerBase
{
    private readonly IColetaService _service;

    public ColetasController(IColetaService service) => _service = service;

    /// <summary>Lista as coletas, com filtros opcionais por situação, cliente e período.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ColetaResponse>>> Listar(
        [FromQuery] StatusColeta? status,
        [FromQuery] int? clienteId,
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim)
    {
        var coletas = await _service.ListarAsync(status, clienteId, inicio, fim);
        return Ok(coletas);
    }

    /// <summary>Busca uma coleta pelo id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ColetaResponse>> ObterPorId(int id)
    {
        var coleta = await _service.ObterPorIdAsync(id);
        return coleta is null ? NotFound() : Ok(coleta);
    }

    /// <summary>Abre uma nova solicitação de coleta.</summary>
    [HttpPost]
    public async Task<ActionResult<ColetaResponse>> Criar([FromBody] CriarColetaRequest request)
    {
        var coleta = await _service.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = coleta.Id }, coleta);
    }

    /// <summary>Atribui motorista e veículo (move a coleta para "Em Coleta").</summary>
    [HttpPost("{id:int}/atribuir")]
    public async Task<IActionResult> Atribuir(int id, [FromBody] AtribuirMotoristaVeiculoRequest request)
    {
        await _service.AtribuirMotoristaEVeiculoAsync(id, request);
        return NoContent();
    }

    /// <summary>Marca a coleta como coletada.</summary>
    [HttpPost("{id:int}/coletar")]
    public async Task<IActionResult> Coletar(int id)
    {
        await _service.MarcarComoColetadaAsync(id);
        return NoContent();
    }

    /// <summary>Cancela a coleta.</summary>
    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _service.CancelarAsync(id);
        return NoContent();
    }

    /// <summary>Registra uma ocorrência na coleta.</summary>
    [HttpPost("{id:int}/ocorrencias")]
    public async Task<IActionResult> RegistrarOcorrencia(int id, [FromBody] RegistrarOcorrenciaRequest request)
    {
        await _service.RegistrarOcorrenciaAsync(id, request);
        return NoContent();
    }
}
