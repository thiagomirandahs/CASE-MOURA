using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Interfaces;
using GestaoColetas.Domain.Entities;

namespace GestaoColetas.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repo;

    public ClienteService(IClienteRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<ClienteResponse>> ListarAsync()
    {
        var clientes = await _repo.ListarAsync();
        return clientes.Select(c => new ClienteResponse(c.Id, c.Nome, c.Documento)).ToList();
    }

    public async Task<ClienteResponse> CriarAsync(CriarClienteRequest req)
    {
        var cliente = new Cliente(req.Nome, req.Documento);
        await _repo.AdicionarAsync(cliente);
        await _repo.SalvarAlteracoesAsync();
        return new ClienteResponse(cliente.Id, cliente.Nome, cliente.Documento);
    }
}
