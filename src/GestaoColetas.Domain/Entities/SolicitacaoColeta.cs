using GestaoColetas.Domain.Enums;

namespace GestaoColetas.Domain.Entities;

/// <summary>
/// Solicitação de coleta — o centro do sistema. Ela controla o próprio status:
/// ele só muda pelos métodos abaixo, que garantem as regras de negócio.
/// </summary>
public class SolicitacaoColeta
{
    public int Id { get; private set; }

    /// <summary>Número único de identificação (ex.: "COL-2026-0001").</summary>
    public string Numero { get; private set; } = string.Empty;

    // Cliente que abriu a solicitação
    public int ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }

    // Quem envia a carga e para quem ela vai
    public string RemetenteNome { get; private set; } = string.Empty;
    public string RemetenteEndereco { get; private set; } = string.Empty;
    public string DestinatarioNome { get; private set; } = string.Empty;
    public string DestinatarioEndereco { get; private set; } = string.Empty;

    public DateTime DataSolicitacao { get; private set; }
    public DateTime DataColetaPrevista { get; private set; }
    public Prioridade Prioridade { get; private set; }
    public string? Observacoes { get; private set; }

    public StatusColeta Status { get; private set; }

    // Motorista e veículo — só preenchidos quando a roteirização atribui
    public int? MotoristaId { get; private set; }
    public Motorista? Motorista { get; private set; }
    public int? VeiculoId { get; private set; }
    public Veiculo? Veiculo { get; private set; }

    private readonly List<Ocorrencia> _ocorrencias = new();
    public IReadOnlyCollection<Ocorrencia> Ocorrencias => _ocorrencias.AsReadOnly();

    // Construtor exigido pelo EF Core (não usar no código).
    protected SolicitacaoColeta() { }

    public SolicitacaoColeta(
        string numero,
        int clienteId,
        string remetenteNome,
        string remetenteEndereco,
        string destinatarioNome,
        string destinatarioEndereco,
        DateTime dataColetaPrevista,
        Prioridade prioridade,
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("O número da coleta é obrigatório.", nameof(numero));
        if (string.IsNullOrWhiteSpace(remetenteNome))
            throw new ArgumentException("O nome do remetente é obrigatório.", nameof(remetenteNome));
        if (string.IsNullOrWhiteSpace(destinatarioNome))
            throw new ArgumentException("O nome do destinatário é obrigatório.", nameof(destinatarioNome));

        Numero = numero;
        ClienteId = clienteId;
        RemetenteNome = remetenteNome;
        RemetenteEndereco = remetenteEndereco;
        DestinatarioNome = destinatarioNome;
        DestinatarioEndereco = destinatarioEndereco;
        DataColetaPrevista = dataColetaPrevista;
        Prioridade = prioridade;
        Observacoes = observacoes;

        DataSolicitacao = DateTime.UtcNow;
        Status = StatusColeta.Aberta; // toda coleta nasce "Aberta"
    }

    /// <summary>
    /// Atribui motorista e veículo e move a coleta para "Em Coleta".
    /// Não permite mexer em coletas canceladas ou já concluídas.
    /// </summary>
    public void AtribuirMotoristaEVeiculo(int motoristaId, int veiculoId)
    {
        if (Status == StatusColeta.Cancelada)
            throw new InvalidOperationException("Não dá para atribuir motorista/veículo a uma coleta cancelada.");
        if (Status == StatusColeta.Coletado)
            throw new InvalidOperationException("Essa coleta já foi concluída.");

        MotoristaId = motoristaId;
        VeiculoId = veiculoId;
        Status = StatusColeta.EmColeta;
    }

    /// <summary>
    /// Marca a coleta como "Coletado". Só funciona com motorista E veículo vinculados.
    /// </summary>
    public void MarcarComoColetada()
    {
        if (Status == StatusColeta.Cancelada)
            throw new InvalidOperationException("Uma coleta cancelada não pode ser coletada.");
        if (MotoristaId is null || VeiculoId is null)
            throw new InvalidOperationException("Não dá para marcar como coletada sem motorista e veículo vinculados.");

        Status = StatusColeta.Coletado;
    }

    /// <summary>
    /// Cancela a coleta. É um estado final: uma coleta cancelada não volta ao fluxo ativo.
    /// </summary>
    public void Cancelar()
    {
        if (Status == StatusColeta.Coletado)
            throw new InvalidOperationException("Uma coleta já concluída não pode ser cancelada.");

        Status = StatusColeta.Cancelada;
    }

    /// <summary>
    /// Registra uma ocorrência. A própria Ocorrencia guarda data/hora e o usuário responsável.
    /// </summary>
    public void RegistrarOcorrencia(string descricao, string usuarioResponsavel)
    {
        _ocorrencias.Add(new Ocorrencia(descricao, usuarioResponsavel));
    }
}
