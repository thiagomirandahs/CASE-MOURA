namespace GestaoColetas.Application.DTOs;

/// <summary>Indicadores das coletas para o painel.</summary>
public record DashboardResponse(
    int Total,
    int Abertas,
    int EmColeta,
    int Coletadas,
    int Canceladas,
    int EmAtraso,
    int AltaPrioridadeAtivas);
