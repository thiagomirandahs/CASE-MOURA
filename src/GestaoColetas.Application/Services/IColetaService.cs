using GestaoColetas.Application.DTOs;
using GestaoColetas.Domain.Enums;

namespace GestaoColetas.Application.Services;

/// <summary>
/// Casos de uso das coletas — o que a aplicação sabe fazer.
/// </summary>
public interface IColetaService
{
    Task<ColetaResponse> CriarAsync(CriarColetaRequest request);

    Task<PagedResult<ColetaResponse>> ListarAsync(
        StatusColeta? status, int? clienteId, DateTime? inicio, DateTime? fim, int pagina, int tamanhoPagina);

    Task<ColetaResponse?> ObterPorIdAsync(int id);

    Task AtribuirMotoristaEVeiculoAsync(int id, AtribuirMotoristaVeiculoRequest request);
    Task MarcarComoColetadaAsync(int id);
    Task CancelarAsync(int id);
    Task RegistrarOcorrenciaAsync(int id, string descricao, string usuarioResponsavel);

    Task<DashboardResponse> ObterDashboardAsync();
}
