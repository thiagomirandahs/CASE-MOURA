namespace GestaoColetas.Application.DTOs;

public record CriarVeiculoRequest(string Placa, string Modelo);

public record VeiculoResponse(int Id, string Placa, string Modelo);
