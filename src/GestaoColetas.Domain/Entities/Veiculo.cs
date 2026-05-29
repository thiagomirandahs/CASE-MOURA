namespace GestaoColetas.Domain.Entities;

/// <summary>
/// Veículo que pode ser atribuído a uma coleta.
/// </summary>
public class Veiculo
{
    public int Id { get; private set; }
    public string Placa { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;

    protected Veiculo() { }

    public Veiculo(string placa, string modelo)
    {
        if (string.IsNullOrWhiteSpace(placa))
            throw new ArgumentException("A placa do veículo é obrigatória.", nameof(placa));
        if (string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("O modelo do veículo é obrigatório.", nameof(modelo));

        Placa = placa;
        Modelo = modelo;
    }
}
