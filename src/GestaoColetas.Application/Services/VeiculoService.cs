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
        var placa = (req.Placa ?? string.Empty).Trim().ToUpperInvariant();
        if (await _repo.PlacaExisteAsync(placa))
            throw new InvalidOperationException("Já existe um veículo cadastrado com essa placa.");

        var veiculo = new Veiculo(placa, req.Modelo); // placa guardada em maiúsculas
        await _repo.AdicionarAsync(veiculo);
        await _repo.SalvarAlteracoesAsync();
        return new VeiculoResponse(veiculo.Id, veiculo.Placa, veiculo.Modelo);
    }
}
