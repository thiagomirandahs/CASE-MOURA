using GestaoColetas.Domain.Entities;
using GestaoColetas.Domain.Enums;
using Xunit;

namespace GestaoColetas.Tests;

/// <summary>
/// Casos de borda da SolicitacaoColeta — validações do construtor e transições menos óbvias.
/// </summary>
public class SolicitacaoColetaBordasTests
{
    private static SolicitacaoColeta NovaColeta() => new(
        "COL-2026-0001", 1, "Remetente", "End A", "Destinatario", "End B",
        DateTime.UtcNow.AddDays(1), Prioridade.Normal);

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void NovaColeta_SemNumero_LancaExcecao(string? numero)
    {
        Assert.Throws<ArgumentException>(() => new SolicitacaoColeta(
            numero!, 1, "Rem", "EndA", "Dest", "EndB", DateTime.UtcNow, Prioridade.Normal));
    }

    [Fact]
    public void NovaColeta_SemRemetente_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new SolicitacaoColeta(
            "COL-1", 1, "", "EndA", "Dest", "EndB", DateTime.UtcNow, Prioridade.Normal));
    }

    [Fact]
    public void NovaColeta_SemDestinatario_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new SolicitacaoColeta(
            "COL-1", 1, "Rem", "EndA", "", "EndB", DateTime.UtcNow, Prioridade.Normal));
    }

    [Fact]
    public void Coletada_NaoAceitaNovaAtribuicao()
    {
        var coleta = NovaColeta();
        coleta.AtribuirMotoristaEVeiculo(1, 1);
        coleta.MarcarComoColetada();

        Assert.Throws<InvalidOperationException>(() => coleta.AtribuirMotoristaEVeiculo(2, 2));
    }

    [Fact]
    public void EmColeta_PodeTrocarMotoristaEVeiculoAntesDeColetar()
    {
        var coleta = NovaColeta();
        coleta.AtribuirMotoristaEVeiculo(1, 1);

        coleta.AtribuirMotoristaEVeiculo(2, 3); // troca antes de coletar

        Assert.Equal(StatusColeta.EmColeta, coleta.Status);
        Assert.Equal(2, coleta.MotoristaId);
        Assert.Equal(3, coleta.VeiculoId);
    }

    [Fact]
    public void EmColeta_PodeSerCancelada()
    {
        var coleta = NovaColeta();
        coleta.AtribuirMotoristaEVeiculo(1, 1);

        coleta.Cancelar();

        Assert.Equal(StatusColeta.Cancelada, coleta.Status);
    }
}
