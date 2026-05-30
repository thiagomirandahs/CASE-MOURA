namespace GestaoColetas.Application.DTOs;

/// <summary>Resultado paginado genérico (itens da página + total geral).</summary>
public record PagedResult<T>(IReadOnlyList<T> Itens, int Total, int Pagina, int TamanhoPagina)
{
    public int TotalPaginas => TamanhoPagina > 0 ? (int)Math.Ceiling((double)Total / TamanhoPagina) : 0;
}
