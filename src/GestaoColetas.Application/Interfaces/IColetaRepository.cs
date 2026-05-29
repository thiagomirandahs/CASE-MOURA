using GestaoColetas.Domain.Entities;
using GestaoColetas.Domain.Enums;

namespace GestaoColetas.Application.Interfaces;

/// <summary>
/// Contrato de acesso a dados das coletas. Diz O QUE o sistema precisa do banco,
/// sem dizer COMO — a implementação fica na camada de Infraestrutura.
/// </summary>
public interface IColetaRepository
{
    Task<SolicitacaoColeta?> ObterPorIdAsync(int id);

    Task<IReadOnlyList<SolicitacaoColeta>> ListarAsync(
        StatusColeta? status, int? clienteId, DateTime? inicio, DateTime? fim);

    Task<int> ContarAsync();

    Task<bool> ClienteExisteAsync(int clienteId);
    Task<bool> MotoristaExisteAsync(int motoristaId);
    Task<bool> VeiculoExisteAsync(int veiculoId);

    Task AdicionarAsync(SolicitacaoColeta coleta);
    Task SalvarAlteracoesAsync();
}
