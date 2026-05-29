using GestaoColetas.Domain.Entities;

namespace GestaoColetas.Application.Interfaces;

/// <summary>
/// Contrato de acesso a dados dos motoristas. Diz O QUE o sistema precisa do banco,
/// sem dizer COMO — a implementação fica na camada de Infraestrutura.
/// </summary>
public interface IMotoristaRepository
{
    Task<IReadOnlyList<Motorista>> ListarAsync();
    Task<Motorista?> ObterPorIdAsync(int id);
    Task AdicionarAsync(Motorista motorista);
    Task SalvarAlteracoesAsync();
}