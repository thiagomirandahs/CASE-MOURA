namespace GestaoColetas.Application.DTOs;

public record CriarClienteRequest(string Nome, string? Documento);

public record ClienteResponse(int Id, string Nome, string? Documento);
