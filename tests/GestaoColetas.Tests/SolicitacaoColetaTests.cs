using GestaoColetas.Domain.Entities;
using GestaoColetas.Domain.Enums;
using Xunit;

namespace GestaoColetas.Tests;

/// <summary>
/// Testes das regras de negócio da coleta — o coração do sistema.
/// Tudo roda no domínio, sem banco: cada teste cria uma coleta e exercita uma regra.
/// </summary>
public class SolicitacaoColetaTests
{
    // Cria uma coleta "Aberta" padrão para reaproveitar nos testes.
    private static SolicitacaoColeta NovaColeta() => new(
        numero: "COL-2026-0001",
        clienteId: 1,
        remetenteNome: "Mercado Central",
        remetenteEndereco: "Av. Brasil, 1000",
        destinatarioNome: "Filial Sul",
        destinatarioEndereco: "Rua D, 80",
        dataColetaPrevista: DateTime.UtcNow.AddDays(1),
        prioridade: Prioridade.Normal);

    // ---------- Estado inicial ----------

    [Fact]
    public void NovaColeta_NasceComStatusAberta()
    {
        var coleta = NovaColeta();
        Assert.Equal(StatusColeta.Aberta, coleta.Status);
    }

    [Fact]
    public void NovaColeta_SemNumero_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new SolicitacaoColeta(
            "", 1, "Remetente", "End", "Destinatario", "End",
            DateTime.UtcNow.AddDays(1), Prioridade.Normal));
    }

    // ---------- Regra: atribuir motorista + veículo move para "Em Coleta" ----------

    [Fact]
    public void Atribuir_MotoristaEVeiculo_MoveParaEmColeta()
    {
        var coleta = NovaColeta();

        coleta.AtribuirMotoristaEVeiculo(motoristaId: 10, veiculoId: 20);

        Assert.Equal(StatusColeta.EmColeta, coleta.Status);
        Assert.Equal(10, coleta.MotoristaId);
        Assert.Equal(20, coleta.VeiculoId);
    }

    // ---------- Regra: só marca "Coletado" com motorista E veículo ----------

    [Fact]
    public void Coletar_ComMotoristaEVeiculo_FicaColetado()
    {
        var coleta = NovaColeta();
        coleta.AtribuirMotoristaEVeiculo(10, 20);

        coleta.MarcarComoColetada();

        Assert.Equal(StatusColeta.Coletado, coleta.Status);
    }

    [Fact]
    public void Coletar_SemMotoristaEVeiculo_LancaExcecao()
    {
        var coleta = NovaColeta(); // continua Aberta, sem motorista/veículo

        Assert.Throws<InvalidOperationException>(() => coleta.MarcarComoColetada());
    }

    // ---------- Regra: cancelada é terminal (não volta ao fluxo) ----------

    [Fact]
    public void Cancelar_DeixaStatusCancelada()
    {
        var coleta = NovaColeta();

        coleta.Cancelar();

        Assert.Equal(StatusColeta.Cancelada, coleta.Status);
    }

    [Fact]
    public void Cancelada_NaoAceitaAtribuirMotoristaEVeiculo()
    {
        var coleta = NovaColeta();
        coleta.Cancelar();

        Assert.Throws<InvalidOperationException>(() => coleta.AtribuirMotoristaEVeiculo(10, 20));
    }

    [Fact]
    public void Cancelada_NaoPodeSerColetada()
    {
        var coleta = NovaColeta();
        coleta.Cancelar();

        Assert.Throws<InvalidOperationException>(() => coleta.MarcarComoColetada());
    }

    [Fact]
    public void Coletada_NaoPodeSerCancelada()
    {
        var coleta = NovaColeta();
        coleta.AtribuirMotoristaEVeiculo(10, 20);
        coleta.MarcarComoColetada();

        Assert.Throws<InvalidOperationException>(() => coleta.Cancelar());
    }

    // ---------- Regra: ocorrência guarda descrição + responsável (+ data/hora) ----------

    [Fact]
    public void RegistrarOcorrencia_AdicionaNaListaComResponsavel()
    {
        var coleta = NovaColeta();

        coleta.RegistrarOcorrencia("Endereço errado", "admin");

        var ocorrencia = Assert.Single(coleta.Ocorrencias);
        Assert.Equal("Endereço errado", ocorrencia.Descricao);
        Assert.Equal("admin", ocorrencia.UsuarioResponsavel);
        Assert.NotEqual(default(DateTime), ocorrencia.DataHora);
    }

    [Fact]
    public void RegistrarOcorrencia_SemDescricao_LancaExcecao()
    {
        var coleta = NovaColeta();

        Assert.Throws<ArgumentException>(() => coleta.RegistrarOcorrencia("", "admin"));
    }

    // ---------- Fluxo feliz completo ----------

    [Fact]
    public void FluxoCompleto_DeAbertaAteColetado()
    {
        var coleta = NovaColeta();
        Assert.Equal(StatusColeta.Aberta, coleta.Status);

        coleta.AtribuirMotoristaEVeiculo(10, 20);
        Assert.Equal(StatusColeta.EmColeta, coleta.Status);

        coleta.MarcarComoColetada();
        Assert.Equal(StatusColeta.Coletado, coleta.Status);
    }
}
