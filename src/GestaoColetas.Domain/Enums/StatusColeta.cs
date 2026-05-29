namespace GestaoColetas.Domain.Enums;

/// <summary>
/// Etapas pelas quais uma solicitação de coleta passa.
/// </summary>
public enum StatusColeta
{
    Aberta = 1,
    EmColeta = 2,
    Coletado = 3,
    Cancelada = 4
}
