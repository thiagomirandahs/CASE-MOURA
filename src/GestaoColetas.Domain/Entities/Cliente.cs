namespace GestaoColetas.Domain.Entities;

/// <summary>
/// Cliente da transportadora — quem abre uma solicitação de coleta.
/// </summary>
public class Cliente
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Documento { get; private set; }

    // Construtor exigido pelo EF Core (não usar no código).
    protected Cliente() { }

    public Cliente(string nome, string? documento = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do cliente é obrigatório.", nameof(nome));

        Nome = nome;
        Documento = documento;
    }
}
