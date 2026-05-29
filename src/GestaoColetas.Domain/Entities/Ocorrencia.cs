namespace GestaoColetas.Domain.Entities;

/// <summary>
/// Registro de algo que aconteceu durante a coleta (ex.: endereço errado,
/// remetente ausente). Guarda sempre data/hora e quem registrou — Regra de negócio.
/// </summary>
public class Ocorrencia
{
    public int Id { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public DateTime DataHora { get; private set; }
    public string UsuarioResponsavel { get; private set; } = string.Empty;

    // Chave estrangeira para a coleta (o EF Core preenche pela relação).
    public int SolicitacaoColetaId { get; private set; }

    protected Ocorrencia() { }

    public Ocorrencia(string descricao, string usuarioResponsavel)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da ocorrência é obrigatória.", nameof(descricao));
        if (string.IsNullOrWhiteSpace(usuarioResponsavel))
            throw new ArgumentException("O usuário responsável é obrigatório.", nameof(usuarioResponsavel));

        Descricao = descricao;
        UsuarioResponsavel = usuarioResponsavel;
        DataHora = DateTime.UtcNow; // registra automaticamente quando aconteceu
    }
}
