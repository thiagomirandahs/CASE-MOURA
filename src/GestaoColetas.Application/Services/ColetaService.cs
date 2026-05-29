using GestaoColetas.Application.DTOs;
using GestaoColetas.Application.Interfaces;
using GestaoColetas.Domain.Entities;
using GestaoColetas.Domain.Enums;

namespace GestaoColetas.Application.Services;

public class ColetaService : IColetaService
{
    private readonly IColetaRepository _repo;

    public ColetaService(IColetaRepository repo) => _repo = repo;

    public async Task<ColetaResponse> CriarAsync(CriarColetaRequest req)
    {
        if (!await _repo.ClienteExisteAsync(req.ClienteId))
            throw new InvalidOperationException("Cliente não encontrado.");

        var numero = await GerarNumeroAsync();
        var coleta = new SolicitacaoColeta(
            numero, req.ClienteId, req.RemetenteNome, req.RemetenteEndereco,
            req.DestinatarioNome, req.DestinatarioEndereco, req.DataColetaPrevista,
            req.Prioridade, req.Observacoes);

        await _repo.AdicionarAsync(coleta);
        await _repo.SalvarAlteracoesAsync();

        // recarrega já com cliente/ocorrências pra montar a resposta completa
        var criada = await _repo.ObterPorIdAsync(coleta.Id);
        return MapToResponse(criada!);
    }

    public async Task<IReadOnlyList<ColetaResponse>> ListarAsync(
        StatusColeta? status, int? clienteId, DateTime? inicio, DateTime? fim)
    {
        var coletas = await _repo.ListarAsync(status, clienteId, inicio, fim);
        return coletas.Select(MapToResponse).ToList();
    }

    public async Task<ColetaResponse?> ObterPorIdAsync(int id)
    {
        var coleta = await _repo.ObterPorIdAsync(id);
        return coleta is null ? null : MapToResponse(coleta);
    }

    public async Task AtribuirMotoristaEVeiculoAsync(int id, AtribuirMotoristaVeiculoRequest req)
    {
        var coleta = await ObterOuFalharAsync(id);

        if (!await _repo.MotoristaExisteAsync(req.MotoristaId))
            throw new InvalidOperationException("Motorista não encontrado.");
        if (!await _repo.VeiculoExisteAsync(req.VeiculoId))
            throw new InvalidOperationException("Veículo não encontrado.");

        coleta.AtribuirMotoristaEVeiculo(req.MotoristaId, req.VeiculoId); // regra no domínio
        await _repo.SalvarAlteracoesAsync();
    }

    public async Task MarcarComoColetadaAsync(int id)
    {
        var coleta = await ObterOuFalharAsync(id);
        coleta.MarcarComoColetada(); // regra no domínio
        await _repo.SalvarAlteracoesAsync();
    }

    public async Task CancelarAsync(int id)
    {
        var coleta = await ObterOuFalharAsync(id);
        coleta.Cancelar(); // regra no domínio
        await _repo.SalvarAlteracoesAsync();
    }

    public async Task RegistrarOcorrenciaAsync(int id, RegistrarOcorrenciaRequest req)
    {
        var coleta = await ObterOuFalharAsync(id);
        coleta.RegistrarOcorrencia(req.Descricao, req.UsuarioResponsavel); // regra no domínio
        await _repo.SalvarAlteracoesAsync();
    }

    private async Task<SolicitacaoColeta> ObterOuFalharAsync(int id)
    {
        var coleta = await _repo.ObterPorIdAsync(id);
        if (coleta is null)
            throw new KeyNotFoundException($"Coleta {id} não encontrada.");
        return coleta;
    }

    private async Task<string> GerarNumeroAsync()
    {
        var proximo = await _repo.ContarAsync() + 1;
        return $"COL-{DateTime.UtcNow:yyyy}-{proximo:D4}";
    }

    private static ColetaResponse MapToResponse(SolicitacaoColeta c) => new(
        c.Id,
        c.Numero,
        c.ClienteId,
        c.Cliente?.Nome,
        c.RemetenteNome,
        c.RemetenteEndereco,
        c.DestinatarioNome,
        c.DestinatarioEndereco,
        c.DataSolicitacao,
        c.DataColetaPrevista,
        c.Prioridade.ToString(),
        c.Status.ToString(),
        c.Prioridade == Prioridade.Alta,
        c.MotoristaId,
        c.Motorista?.Nome,
        c.VeiculoId,
        c.Veiculo?.Placa,
        c.Observacoes,
        c.Ocorrencias
            .Select(o => new OcorrenciaResponse(o.Id, o.Descricao, o.DataHora, o.UsuarioResponsavel))
            .ToList());
}
