using GestaoColetas.Application.DTOs;

namespace GestaoColetas.Application.Services;

/// <summary>
/// Casos de uso dos motoristas — o que a aplicação sabe fazer.
/// </summary>
public interface IMotoristaService
{
    Task<IReadOnlyList<MotoristaResponse>> ListarAsync();
    Task<MotoristaResponse?> ObterPorIdAsync(int id);
    Task<MotoristaResponse> CriarAsync(CriarMotoristaRequest request);
}