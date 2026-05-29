namespace GestaoColetas.Domain.Entities;

/// <summary>
/// Motorista que pode ser atribuído a uma coleta.
/// </summary>
public class Motorista
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cnh { get; private set; } = string.Empty;

    protected Motorista() { }

    public Motorista(string nome, string cnh)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do motorista é obrigatório.", nameof(nome));
        if (string.IsNullOrWhiteSpace(cnh))
            throw new ArgumentException("A CNH do motorista é obrigatória.", nameof(cnh));

        Nome = nome;
        Cnh = cnh;
    }
}
