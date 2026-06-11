using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Interfaces;
using GestaoColetas.Domain.Entities;

namespace GestaoColetas.Application.Services;

public class MotoristaService : IMotoristaService
{
    private readonly IMotoristaRepository _repo;

    public MotoristaService(IMotoristaRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<MotoristaResponse>> ListarAsync()
    {
        var motoristas = await _repo.ListarAsync();
        return motoristas.Select(MapToResponse).ToList();
    }

    public async Task<MotoristaResponse?> ObterPorIdAsync(int id)
    {
        var motorista = await _repo.ObterPorIdAsync(id);
        return motorista is null ? null : MapToResponse(motorista);
    }

    public async Task<MotoristaResponse> CriarAsync(CriarMotoristaRequest req)
    {
        var cnh = req.Cnh?.Trim() ?? string.Empty;
        if (await _repo.CnhExisteAsync(cnh))
            throw new InvalidOperationException("Já existe um motorista cadastrado com essa CNH.");

        var motorista = new Motorista(req.Nome, cnh); // a validação dos campos mora no domínio
        await _repo.AdicionarAsync(motorista);
        await _repo.SalvarAlteracoesAsync();
        return MapToResponse(motorista);
    }

    private static MotoristaResponse MapToResponse(Motorista m) =>
        new(m.Id, m.Nome, m.Cnh);
}