using GestaoColetas.Domain.Enums;

namespace GestaoColetas.Application.DTOs;

// Entrada: dados pra abrir uma nova coleta (o número é gerado pelo sistema).
public record CriarColetaRequest(
    int ClienteId,
    string RemetenteNome,
    string RemetenteEndereco,
    string DestinatarioNome,
    string DestinatarioEndereco,
    DateTime DataColetaPrevista,
    Prioridade Prioridade,
    string? Observacoes);

// Entrada: atribuir motorista e veículo a uma coleta.
public record AtribuirMotoristaVeiculoRequest(int MotoristaId, int VeiculoId);

// Entrada: registrar uma ocorrência.
public record RegistrarOcorrenciaRequest(string Descricao, string UsuarioResponsavel);

// Saída: uma ocorrência.
public record OcorrenciaResponse(int Id, string Descricao, DateTime DataHora, string UsuarioResponsavel);

// Saída: uma coleta completa (o que a API devolve).
public record ColetaResponse(
    int Id,
    string Numero,
    int ClienteId,
    string? ClienteNome,
    string RemetenteNome,
    string RemetenteEndereco,
    string DestinatarioNome,
    string DestinatarioEndereco,
    DateTime DataSolicitacao,
    DateTime DataColetaPrevista,
    string Prioridade,
    string Status,
    bool PrioridadeAlta,
    int? MotoristaId,
    string? MotoristaNome,
    int? VeiculoId,
    string? VeiculoPlaca,
    string? Observacoes,
    IReadOnlyList<OcorrenciaResponse> Ocorrencias);
