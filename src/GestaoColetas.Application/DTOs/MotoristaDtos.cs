namespace GestaoColetas.Application.DTOs;

// Entrada: dados pra cadastrar um motorista novo (o Id é gerado pelo banco)
public record CriarMotoristaRequest(string Nome, string Cnh);

// Saída: um motorista (o que a API devolve)
public record MotoristaResponse(int Id, string Nome, string Cnh);