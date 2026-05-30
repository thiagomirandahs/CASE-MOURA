using GestaoColetas.Application.DTOs;

namespace GestaoColetas.Application.Services;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteResponse>> ListarAsync();
    Task<ClienteResponse> CriarAsync(CriarClienteRequest request);
}
