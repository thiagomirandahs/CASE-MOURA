using GestaoColetas.Domain.Entities;
using Xunit;

namespace GestaoColetas.Tests;

/// <summary>
/// Testes da entidade Ocorrencia — ela exige descrição e responsável, e carimba a data/hora.
/// </summary>
public class OcorrenciaTests
{
    [Fact]
    public void NovaOcorrencia_GuardaDescricaoResponsavelEDataHora()
    {
        var antes = DateTime.UtcNow.AddSeconds(-2);

        var o = new Ocorrencia("Endereço errado", "admin");

        Assert.Equal("Endereço errado", o.Descricao);
        Assert.Equal("admin", o.UsuarioResponsavel);
        Assert.True(o.DataHora >= antes && o.DataHora <= DateTime.UtcNow.AddSeconds(2));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NovaOcorrencia_SemDescricao_LancaExcecao(string? descricao)
    {
        Assert.Throws<ArgumentException>(() => new Ocorrencia(descricao!, "admin"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NovaOcorrencia_SemResponsavel_LancaExcecao(string? usuario)
    {
        Assert.Throws<ArgumentException>(() => new Ocorrencia("Descrição válida", usuario!));
    }
}
