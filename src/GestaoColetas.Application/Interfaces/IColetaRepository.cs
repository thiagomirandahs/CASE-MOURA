using GestaoColetas.Application.DTOs;
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

    Task<(IReadOnlyList<SolicitacaoColeta> Itens, int Total)> ListarAsync(
        StatusColeta? status, int? clienteId, DateTime? inicio, DateTime? fim, int pagina, int tamanhoPagina);

    // Mesmos filtros da listagem, porém sem paginar — usado na exportação.
    Task<IReadOnlyList<SolicitacaoColeta>> ListarTodasAsync(
        StatusColeta? status, int? clienteId, DateTime? inicio, DateTime? fim);

    Task<int> ContarAsync();

    Task<bool> ClienteExisteAsync(int clienteId);
    Task<bool> MotoristaExisteAsync(int motoristaId);
    Task<bool> VeiculoExisteAsync(int veiculoId);

    Task AdicionarAsync(SolicitacaoColeta coleta);
    Task SalvarAlteracoesAsync();

    Task<DashboardResponse> ObterDashboardAsync();
}
