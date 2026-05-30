using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Interfaces;
using GestaoColetas.Domain.Entities;

namespace GestaoColetas.Application.Services;

public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoRepository _repo;

    public VeiculoService(IVeiculoRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<VeiculoResponse>> ListarAsync()
    {
        var veiculos = await _repo.ListarAsync();
        return veiculos.Select(v => new VeiculoResponse(v.Id, v.Placa, v.Modelo)).ToList();
    }

    public async Task<VeiculoResponse> CriarAsync(CriarVeiculoRequest req)
    {
        var veiculo = new Veiculo(req.Placa, req.Modelo);
        await _repo.AdicionarAsync(veiculo);
        await _repo.SalvarAlteracoesAsync();
        return new VeiculoResponse(veiculo.Id, veiculo.Placa, veiculo.Modelo);
    }
}
