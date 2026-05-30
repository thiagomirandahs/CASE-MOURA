using System.Text;
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

    public async Task<PagedResult<ColetaResponse>> ListarAsync(
        StatusColeta? status, int? clienteId, DateTime? inicio, DateTime? fim, int pagina, int tamanhoPagina)
    {
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 20;

        var (itens, total) = await _repo.ListarAsync(status, clienteId, inicio, fim, pagina, tamanhoPagina);
        var resposta = itens.Select(MapToResponse).ToList();
        return new PagedResult<ColetaResponse>(resposta, total, pagina, tamanhoPagina);
    }

    public async Task<string> ExportarCsvAsync(
        StatusColeta? status, int? clienteId, DateTime? inicio, DateTime? fim)
    {
        var coletas = (await _repo.ListarTodasAsync(status, clienteId, inicio, fim))
            .Select(MapToResponse)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(';', new[]
        {
            "Numero", "Cliente", "Remetente", "Endereco Remetente", "Destinatario", "Endereco Destinatario",
            "Prioridade", "Status", "Em Atraso", "Data Solicitacao", "Data Prevista",
            "Motorista", "Veiculo", "Qtd Ocorrencias", "Ocorrencias", "Observacoes"
        }));

        foreach (var c in coletas)
        {
            // Junta todas as ocorrências da coleta numa célula só: "data - usuario: descricao | ..."
            var ocorrencias = string.Join(" | ", c.Ocorrencias.Select(o =>
                $"{o.DataHora:dd/MM/yyyy HH:mm} - {o.UsuarioResponsavel}: {o.Descricao}"));

            sb.AppendLine(string.Join(';', new[]
            {
                Csv(c.Numero),
                Csv(c.ClienteNome),
                Csv(c.RemetenteNome),
                Csv(c.RemetenteEndereco),
                Csv(c.DestinatarioNome),
                Csv(c.DestinatarioEndereco),
                Csv(c.Prioridade),
                Csv(c.Status),
                c.EmAtraso ? "Sim" : "Nao",
                c.DataSolicitacao.ToString("dd/MM/yyyy"),
                c.DataColetaPrevista.ToString("dd/MM/yyyy"),
                Csv(c.MotoristaNome),
                Csv(c.VeiculoPlaca),
                c.Ocorrencias.Count.ToString(),
                Csv(ocorrencias),
                Csv(c.Observacoes)
            }));
        }

        return sb.ToString();
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

    public async Task RegistrarOcorrenciaAsync(int id, string descricao, string usuarioResponsavel)
    {
        var coleta = await ObterOuFalharAsync(id);
        coleta.RegistrarOcorrencia(descricao, usuarioResponsavel); // regra no domínio
        await _repo.SalvarAlteracoesAsync();
    }

    public Task<DashboardResponse> ObterDashboardAsync() => _repo.ObterDashboardAsync();

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

    // Escapa um campo de texto para CSV: se tiver ; " ou quebra de linha, envolve em aspas.
    private static string Csv(string? valor)
    {
        valor ??= "";
        if (valor.Contains(';') || valor.Contains('"') || valor.Contains('\n') || valor.Contains('\r'))
            return "\"" + valor.Replace("\"", "\"\"") + "\"";
        return valor;
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
        c.Status != StatusColeta.Coletado && c.Status != StatusColeta.Cancelada && c.DataColetaPrevista < DateTime.UtcNow,
        c.MotoristaId,
        c.Motorista?.Nome,
        c.VeiculoId,
        c.Veiculo?.Placa,
        c.Observacoes,
        c.Ocorrencias
            .Select(o => new OcorrenciaResponse(o.Id, o.Descricao, o.DataHora, o.UsuarioResponsavel))
            .ToList());
}
