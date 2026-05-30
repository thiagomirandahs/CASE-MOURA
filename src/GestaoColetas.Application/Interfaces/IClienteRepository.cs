using GestaoColetas.Domain.Entities;

namespace GestaoColetas.Application.Interfaces;

public interface IClienteRepository
{
    Task<IReadOnlyList<Cliente>> ListarAsync();
    Task<Cliente?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Cliente cliente);
    Task SalvarAlteracoesAsync();
}
