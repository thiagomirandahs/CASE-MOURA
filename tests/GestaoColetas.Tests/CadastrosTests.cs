using GestaoColetas.Domain.Entities;
using Xunit;

namespace GestaoColetas.Tests;

/// <summary>
/// Testes das entidades de cadastro: Cliente, Motorista e Veiculo (suas validações).
/// </summary>
public class CadastrosTests
{
    // ---------- Cliente ----------

    [Fact]
    public void NovoCliente_GuardaNomeEDocumento()
    {
        var c = new Cliente("Mercado Central", "12.345.678/0001-90");
        Assert.Equal("Mercado Central", c.Nome);
        Assert.Equal("12.345.678/0001-90", c.Documento);
    }

    [Fact]
    public void NovoCliente_DocumentoEhOpcional()
    {
        var c = new Cliente("Cliente sem CNPJ");
        Assert.Null(c.Documento);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void NovoCliente_SemNome_LancaExcecao(string? nome)
    {
        Assert.Throws<ArgumentException>(() => new Cliente(nome!));
    }

    // ---------- Motorista ----------

    [Fact]
    public void NovoMotorista_GuardaNomeECnh()
    {
        var m = new Motorista("Carlos Souza", "12345678900");
        Assert.Equal("Carlos Souza", m.Nome);
        Assert.Equal("12345678900", m.Cnh);
    }

    [Fact]
    public void NovoMotorista_SemNome_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Motorista("", "12345678900"));
    }

    [Fact]
    public void NovoMotorista_SemCnh_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Motorista("Carlos Souza", ""));
    }

    // ---------- Veiculo ----------

    [Fact]
    public void NovoVeiculo_GuardaPlacaEModelo()
    {
        var v = new Veiculo("ABC1D23", "Fiat Fiorino");
        Assert.Equal("ABC1D23", v.Placa);
        Assert.Equal("Fiat Fiorino", v.Modelo);
    }

    [Fact]
    public void NovoVeiculo_SemPlaca_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Veiculo("", "Fiat Fiorino"));
    }

    [Fact]
    public void NovoVeiculo_SemModelo_LancaExcecao()
    {
        Assert.Throws<ArgumentException>(() => new Veiculo("ABC1D23", ""));
    }
}
