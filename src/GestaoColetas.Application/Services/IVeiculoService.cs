using GestaoColetas.Application.DTOs;

namespace GestaoColetas.Application.Services;

public interface IVeiculoService
{
    Task<IReadOnlyList<VeiculoResponse>> ListarAsync();
    Task<VeiculoResponse> CriarAsync(CriarVeiculoRequest request);
}
