using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColetas.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IColetaService _service;

    public DashboardController(IColetaService service) => _service = service;

    /// <summary>Indicadores das coletas: totais por status, em atraso e alta prioridade ativa.</summary>
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Obter() => Ok(await _service.ObterDashboardAsync());
}
