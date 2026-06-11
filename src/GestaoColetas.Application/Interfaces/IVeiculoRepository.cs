using GestaoColetas.Domain.Entities;

namespace GestaoColetas.Application.Interfaces;

public interface IVeiculoRepository
{
    Task<IReadOnlyList<Veiculo>> ListarAsync();
    Task<Veiculo?> ObterPorIdAsync(int id);
    Task<bool> PlacaExisteAsync(string placa);
    Task AdicionarAsync(Veiculo veiculo);
    Task SalvarAlteracoesAsync();
}
